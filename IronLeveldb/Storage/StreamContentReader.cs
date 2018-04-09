using System.IO;

namespace IronLeveldb.Storage
{
    public class StreamContentReader : IContentReader
    {
        private readonly object _lock = new object();
        private readonly Stream _stream;

        public StreamContentReader(Stream stream)
        {
            _stream = stream;
            ContentLength = stream.Length;
        }

        public long ContentLength { get; }

        public int ReadContentInto(long pos, byte[] buffer, int offset, int size)
        {
            lock (_lock)
            {
                _stream.Position = pos;
                return _stream.Read(buffer, offset, size);
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
