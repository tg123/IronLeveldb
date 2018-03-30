using System.IO;

namespace IronLevelDB.Storages
{
    public class StreamContentReader : IContentReader
    {
        private readonly BinaryReader _br;

        private readonly object _lock = new object();
        private readonly Stream _stream;

        public StreamContentReader(Stream stream)
        {
            _stream = stream;
            _br = new BinaryReader(stream);
        }

        public long ContentLength => _stream.Length;

        public byte[] ReadContent(long offset, long size)
        {
            lock (_lock)
            {
                _stream.Position = offset;
                // TODO cast
                return _br.ReadBytes((int) size);
            }
        }
    }
}
