using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IronLevelDB.SSTable;

namespace IronLevelDB.DB
{
    internal class Version : ISeekable<InternalKey, InternalIByteArrayKeyValuePair>, IDisposable
    {
        private readonly ICache _cache;

        private readonly long _cacheId;

        private readonly List<FileMetaData>[] _files = Enumerable.Range(0, Config.NumLevels)
            .Select(_ => new List<FileMetaData>()).ToArray();

        private readonly IComparer<InternalIByteArrayKeyValuePair> _internalComparer;

        private readonly InternalKeyComparer _internalKeyComparer;

        private readonly IIronLeveldbOptions _options;

        private List<FileMetaData> _level0 = new List<FileMetaData>();
        private List<FileMetaData> _level1AndLarger = new List<FileMetaData>();

        private Version(IIronLeveldbOptions options)
        {
            _options = options;
            _internalKeyComparer = new InternalKeyComparer(options.Comparer);
            _internalComparer = Comparer<InternalIByteArrayKeyValuePair>.Create(
                (a, b) => _internalKeyComparer.Compare(a.InternalKey, b.InternalKey));

            _cacheId = options.TableCache.NewId();

            _cache = options.TableCache;
        }


        public void Dispose()
        {
            foreach (var fileMetaData in _files.SelectMany(l => l))
            {
                var cacheKey = ToCacheKey(fileMetaData);
                _cache.Erase(_cacheId, cacheKey);
            }
        }

        public IEnumerable<InternalIByteArrayKeyValuePair> Seek(InternalKey key)
        {
            // TODO only seek level0 and overlaps
            var entries = _level0.Concat(_level1AndLarger)
                .Where(f => _internalKeyComparer.Compare(key, f.Largest) <= 0)
                .Select(f =>
                {
                    // key < smallest
                    if (_internalKeyComparer.Compare(key, f.Smallest) <= 0)
                    {
                        return ToTable(f).SeekFirst();
                    }

//                    return ToTable(f).Seek(interkey.ToByteArray()).ToInternalKeyValuePairs();
                    return ToTable(f).Seek(key);
                });

            return Merge(entries);
        }

        public IEnumerable<InternalIByteArrayKeyValuePair> SeekFirst()
        {
            var entries = _level0.Concat(_level1AndLarger)
                .Select(ToTable).Select(t => t.SeekFirst());

            return Merge(entries);
        }

        private void UpdateLevelCache()
        {
            _level0 = _files[0].OrderByDescending(f => f.Number).ToList();
            _level1AndLarger = _files.Where((f, i) => i > 0).SelectMany(l => l).ToList();
        }

        private IEnumerable<InternalIByteArrayKeyValuePair> Merge(
            IEnumerable<IEnumerable<InternalIByteArrayKeyValuePair>> entries)
        {
            var a = entries.ToArray();

            if (a.Length == 0)
            {
                return Enumerable.Empty<InternalIByteArrayKeyValuePair>();
            }

            if (a.Length == 1)
            {
                return a[0];
            }

            return new MergingEnumerable<InternalIByteArrayKeyValuePair>(a, _internalComparer);
        }

        private byte[] ToCacheKey(FileMetaData file)
        {
            return BitConverter.GetBytes(file.Number);
        }

        private Table ToTable(FileMetaData file)
        {
            var cacheKey = ToCacheKey(file);
            var table = _cache.Lookup<Table>(_cacheId, cacheKey);
            if (table == null)
            {
                var contentReader = _options.Storge.GetTableContentById(file.Number);

                table = new Table(contentReader, _options.BlockCache, _internalKeyComparer);
                _cache.Insert(_cacheId, cacheKey, table);
            }

            return table;
        }


        internal class Builder
        {
            private readonly Version _baseVersion;
            private readonly InternalKeyComparer _internalKeyComparer;

            private readonly LevelState[] _levels;

            private readonly IIronLeveldbOptions _options;


            public Builder(IIronLeveldbOptions options, Version baseVersion)
            {
                _options = options;
                _baseVersion = baseVersion;
                _internalKeyComparer = new InternalKeyComparer(options.Comparer);

                _levels = Enumerable.Range(0, Config.NumLevels)
                    .Select(_ => new LevelState(_internalKeyComparer)).ToArray();
            }

            public Builder With(VersionEdit versionEdit)
            {
                // TODO Update compaction pointers


                // Delete files
                foreach (var iter in versionEdit.DeletedFiles)
                {
                    var level = iter.Item1;
                    var number = iter.Item2;

                    _levels[level].DeletedFiles.Add(number);
                }

                // Add new files
                foreach (var iter in versionEdit.NewFiles)
                {
                    var level = iter.Item1;
                    var f = iter.Item2;

                    _levels[level].AddedFiles.Add(f);
                    _levels[level].DeletedFiles.Remove(f.Number);
                }

                return this;
            }

            public Version Build()
            {
                var v = new Version(_options);
                for (var level = 0; level < Config.NumLevels; level++)
                {
                    // TODO Add all smaller files listed in base_

                    foreach (var addedFile in _levels[level].AddedFiles)
                    {
                        MaybeAddFile(v, level, addedFile);
                    }
                }

                v.UpdateLevelCache();

                return v;
            }

            private void MaybeAddFile(Version version, int level, FileMetaData addedFile)
            {
                if (_levels[level].DeletedFiles.Contains(addedFile.Number))
                {
                    return;
                }

                var files = version._files[level];

                if (level > 0)
                {
                    // Must not overlap
                    if (files.Count > 0)
                    {
                        Debug.Assert(_internalKeyComparer.Compare(files.Last().Largest, addedFile.Smallest) < 0);
                    }
                }

                files.Add(addedFile);
            }

            private class LevelState
            {
                internal readonly SortedSet<FileMetaData> AddedFiles;

                internal readonly HashSet<ulong> DeletedFiles = new HashSet<ulong>();

                public LevelState(InternalKeyComparer internalKeyComparer)
                {
                    AddedFiles = new SortedSet<FileMetaData>(Comparer<FileMetaData>.Create(
                        (f1, f2) =>
                        {
                            var r = internalKeyComparer.Compare(f1.Smallest, f2.Smallest);

                            if (r != 0)
                            {
                                return r;
                            }

                            // Break ties by file number
                            return (int) (f1.Number - f2.Number);
                        }
                    ));
                }
            }
        }
    }
}