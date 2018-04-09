using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//using System.IO.MemoryMappedFiles;

namespace IronLeveldb.Storage.FileSystem
{
    public class ReadonlyFileSystemStorage : IIronLeveldbStorge
    {
        private readonly FileSystemFileNameMaker _fileSystemFileNameMaker;

        private readonly ConcurrentBag<WeakReference<IDisposable>> _opened =
            new ConcurrentBag<WeakReference<IDisposable>>();

        private volatile bool _disposed;

        public ReadonlyFileSystemStorage(string path)
        {
            _fileSystemFileNameMaker = new FileSystemFileNameMaker(path);
        }

        public IContentReader GetCurrentDescriptorContent()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ReadonlyFileSystemStorage));
            }

            var current = File.ReadLines(_fileSystemFileNameMaker.CurrentFileName()).FirstOrDefault()?.Trim();

            // TODO why check new line?
            if (string.IsNullOrEmpty(current))
            {
                throw new InvalidDataException("bad CURRENT file");
            }

            return new StreamContentReader(OpenRead(_fileSystemFileNameMaker.FullPath(current)));
        }

        public IContentReader GetTableContentById(ulong num)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ReadonlyFileSystemStorage));
            }

            return new StreamContentReader(OpenRead(_fileSystemFileNameMaker.TableFileName(num)));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            var cs = new List<Exception>();
            for (;;)
            {
                if (!_opened.TryTake(out var streamRef))
                {
                    if (cs.Count > 1)
                    {
                        throw new AggregateException(cs);
                    }

                    if (cs.Count == 1)
                    {
                        throw cs.First();
                    }

                    break;
                }

                if (streamRef.TryGetTarget(out var stream))
                {
                    try
                    {
                        stream.Dispose();
                    }
                    catch (Exception e)
                    {
                        cs.Add(e);
                    }
                }
            }
        }

        private Stream OpenRead(string file)
        {
            return RegisterForDispose(File.OpenRead(file));
            // TODO mmap
//            var len = new FileInfo(file).Length;
//            var memoryMappedFile = RegisterForDispose(MemoryMappedFile.CreateFromFile(file, FileMode.Open));
//            return RegisterForDispose(memoryMappedFile.CreateViewStream(0, len, MemoryMappedFileAccess.Read));
        }

        private T RegisterForDispose<T>(T obj) where T : IDisposable
        {
            _opened.Add(new WeakReference<IDisposable>(obj));
            return obj;
        }
    }
}
