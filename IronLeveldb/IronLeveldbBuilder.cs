using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronLevelDB.DB;
using IronLevelDB.SSTable;
using IronLevelDB.Storages;
using IronLevelDB.Storages.FileSystem;
using Version = IronLevelDB.DB.Version;

namespace IronLevelDB
{
    public static class IronLeveldbBuilder
    {
        public static IIronLeveldb Build(this IIronLeveldbOptions options)
        {
            var storge = options.Storge;
            var manifestStream = storge.GetCurrentDescriptorContent();

            // TODO base version
            var b = new Version.Builder(options, null);
            using (manifestStream)
            {
                foreach (var stream in new RecoverLogRecordsStream(manifestStream))
                {
                    var versionEdit = VersionEdit.DecodeFrom(stream);

                    b.With(versionEdit);
                }
            }

            var current = b.Build();
            return new IronLeveldbStub(options, current, () =>
            {
                var es = new List<Exception>();
                try
                {
                    storge.Dispose();
                }
                catch (Exception e)
                {
                    es.Add(e);
                }

                try
                {
                    current.Dispose();
                }
                catch (Exception e)
                {
                    es.Add(e);
                }

                if (es.Count == 2)
                {
                    throw new AggregateException(es);
                }

                if (es.Count == 1)
                {
                    throw es.First();
                }
            });
        }

        public static IIronLeveldb BuildFromPath(IIronLeveldbOptions options, string path)
        {
            return Build(new OverwrittingStorageIronLeveldbOptions(options, new ReadonlyFileSystemStorage(path)));
        }

        public static IIronLeveldb BuildFromPath(string path)
        {
            return BuildFromPath(new IronLeveldbOptions(), path);
        }


        public static IIronLeveldb BuildFromSingleTable(IIronLeveldbOptions options, Stream stream)
        {
            var table = new Table(new StreamContentReader(stream), options.BlockCache,
                new InternalKeyComparer(options.Comparer));
            return new IronLeveldbStub(options, table, stream.Dispose);
        }

        public static IIronLeveldb BuildFromSingleTable(Stream stream)
        {
            return BuildFromSingleTable(new IronLeveldbOptions(), stream);
        }

        public static IIronLeveldb BuildFromSingleTable(Stream stream, ICache blockCache)
        {
            return BuildFromSingleTable(new IronLeveldbOptions
            {
                BlockCache = blockCache
            }, stream);
        }

        private class OverwrittingStorageIronLeveldbOptions : IIronLeveldbOptions
        {
            private readonly IIronLeveldbOptions _options;

            public OverwrittingStorageIronLeveldbOptions(IIronLeveldbOptions options, IIronLeveldbStorge storge)
            {
                _options = options;
                Storge = storge;
            }

            public IKeyComparer Comparer => _options.Comparer;

            public IIronLeveldbStorge Storge { get; }

            public ICache TableCache => _options.TableCache;

            public ICache BlockCache => _options.BlockCache;
        }
    }
}
