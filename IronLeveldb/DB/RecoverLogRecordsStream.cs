using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IronLeveldb.DB
{
    internal class RecoverLogRecordsStream : IEnumerable<Stream>
    {
        private const RecordType MaxRecordType = RecordType.LastType;

        // private const int BlockSize = 32768;

        // Header is checksum (4 bytes), length (2 bytes), type (1 byte).
        // private const int HeaderSize = 4 + 2 + 1;
        // move type to body for easy crc32
        private const int HeaderSize = 4 + 2;
        private readonly bool _checksum;

        private readonly Stream _stream;
        private readonly object _streamLock = new object();

        // TODO need do more
        public RecoverLogRecordsStream(Stream stream, bool checksum)
        {
            _stream = stream;
            _checksum = checksum;
        }

        // TODO testcases needed
        public IEnumerator<Stream> GetEnumerator()
        {
            var nextpos = 0L;

            MemoryStream recordStream = null;

            while (nextpos < _stream.Length)
            {
                byte[] header;
                RecordType type;
                byte[] data;
                int length;

                lock (_streamLock)
                {
                    _stream.Position = nextpos;
                    using (var br = new BinaryReader(_stream, Encoding.UTF8, true))
                    {
                        header = br.ReadBytes(HeaderSize);

                        var a = header[4] & 0xff;
                        var b = header[5] & 0xff;
                        length = a | (b << 8);
//                        type = (RecordType) header[6];

                        data = br.ReadBytes(length + 1);

                        if (data.Length != length + 1)
                        {
                            throw new IOException("not fully read");
                        }
                    }

                    nextpos = _stream.Position;
                }

                if (_checksum)
                {
                    var crc = Crc32.ReadUnmaskCrc(header, 0);
                    var actual = Crc32.Value(data, 0, data.Length);

                    if (crc != actual)
                    {
                        throw new InvalidDataException("checksum mismatch");
                    }
                }

                type = (RecordType) data[0];

                switch (type)
                {
                    case RecordType.ZeroType:
                        break;
                    case RecordType.FullType:

                        if (recordStream != null)
                        {
                            throw new InvalidDataException("partial record without end(1)");
                        }

                        yield return new MemoryStream(data, 1, length, false);

                        break;
                    case RecordType.FirstType:
                        if (recordStream != null)
                        {
                            throw new InvalidDataException("partial record without end(2)");
                        }

                        recordStream = new MemoryStream(data, 1, length, true);

                        break;
                    case RecordType.MiddleType:
                        if (recordStream == null)
                        {
                            throw new InvalidDataException("missing start of fragmented record(1)");
                        }

                        recordStream.Write(data, 1, length);

                        break;
                    case RecordType.LastType:
                        if (recordStream == null)
                        {
                            throw new InvalidDataException("missing start of fragmented record(2)");
                        }

                        recordStream.Write(data, 1, length);

                        yield return recordStream;
                        recordStream = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private enum RecordType
        {
            // Zero is reserved for preallocated files
            ZeroType = 0,

            FullType = 1,

            // For fragments
            FirstType = 2,
            MiddleType = 3,
            LastType = 4,

            Eof = MaxRecordType + 1,
            BadRecord = MaxRecordType + 2,
        }
    }
}
