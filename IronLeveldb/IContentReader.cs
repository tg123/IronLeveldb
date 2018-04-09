using System;
using System.IO;

namespace IronLeveldb
{
    // support async ?
    public interface IContentReader : IDisposable
    {
        long ContentLength { get; }

        // same as stream.read but thread safe with pos
        int ReadContentInto(long pos, byte[] buffer, int offset, int size);
    }

    public static class ContentReaderExt
    {
        public static byte[] ReadContent(this IContentReader reader, long pos, int size)
        {
            var buffer = new byte[size];

            var read = 0;

            int len;
            while ((len = reader.ReadContentInto(pos + read, buffer, read, size - read)) > 0)
            {
                read += len;
                if (read == size)
                {
                    return buffer;
                }
            }

            Array.Resize(ref buffer, read);
            return buffer;
        }

        public static Stream AsStream(this IContentReader reader)
        {
            return new ContentReaderStream(reader);
        }

        private class ContentReaderStream : Stream
        {
            private readonly IContentReader _reader;

            public ContentReaderStream(IContentReader reader)
            {
                _reader = reader;
            }

            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;
            public override long Length => _reader.ContentLength;
            public override long Position { get; set; }

            protected override void Dispose(bool disposing)
            {
                _reader.Dispose();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var len = _reader.ReadContentInto(Position, buffer, offset, count);
                Position += len;
                return len;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        Position = offset;
                        break;
                    case SeekOrigin.Current:
                        Position += offset;
                        break;
                    case SeekOrigin.End:
                        Position = Length + offset;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
                }

                return Position;
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
    }
}
