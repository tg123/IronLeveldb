using System.IO;
using System.IO.MemoryMappedFiles;
using IronLeveldb.Storage.FileSystem;

namespace IronLeveldb.Storage.Mmap
{
    public class ReadonlyMemoryMappedStorage : ReadonlyFileSystemStorage
    {
        private readonly FileSystemFileNameMaker _fileSystemFileNameMaker;

        public ReadonlyMemoryMappedStorage(string path) : base(path)
        {
            _fileSystemFileNameMaker = new FileSystemFileNameMaker(path);
        }

        public override IContentReader GetTableContentById(ulong num)
        {
            var fileName = _fileSystemFileNameMaker.TableFileName(num);
            var file = new FileInfo(fileName);

            var mmap = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open);
            return DeferForDispose(new MemoryMappedViewReader(file.Length, mmap));
        }

        private class MemoryMappedViewReader : IContentReader
        {
            private readonly MemoryMappedFile _mmap;
            private readonly MemoryMappedViewAccessor _view;

            public MemoryMappedViewReader(long fileLength, MemoryMappedFile mmap)
            {
                ContentLength = fileLength;
                _mmap = mmap;
                _view = mmap.CreateViewAccessor();
            }

            public void Dispose()
            {
                _view.Dispose();
                _mmap.Dispose();
            }

            public long ContentLength { get; }

            public int ReadContentInto(long pos, byte[] buffer, int offset, int size)
            {
                return _view.ReadArray(pos, buffer, offset, size);
            }
        }
    }
}
