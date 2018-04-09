using System;
using System.IO;
using Snappy.Sharp;

namespace IronLeveldb.SSTable
{
    internal static class BlockContentReaderExt
    {
        // 1-byte type + 32-bit crc
        private const int BlockTrailerSize = 5;

        public static byte[] ReadBlock(this IContentReader reader, BlockHandle handle)
        {
//            var br = new BinaryReader(stream);
//            stream.Position = handle.Offset;

            // TODO optimize mem usage
            // TODO max int byte[]

            var n = (int) handle.Size;
            var data = reader.ReadContent(handle.Offset, n + BlockTrailerSize);

            if (data.Length != n + BlockTrailerSize)
            {
                throw new InvalidDataException("truncated block read");
            }

            // TODO crc32 checksum


            switch ((CompressionType) data[n])
            {
                case CompressionType.NoCompression:
                    Array.Resize(ref data, n);
                    return data;
                //                    break;
                case CompressionType.SnappyCompression:
//                                        Array.Resize(ref data, n);

                    // TODO impl should not be hardcode
                    var decompressor = new SnappyDecompressor();
                    return decompressor.Decompress(data, 0, n);
                //                    break;
                default:
                    throw new InvalidDataException("bad block type");
            }
        }

        private enum CompressionType
        {
            // NOTE: do not change the values of existing entries, as these are
            // part of the persistent format on disk.
            NoCompression = 0x0,
            SnappyCompression = 0x1
        }
    }
}
