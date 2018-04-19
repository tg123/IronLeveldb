#if NET45
using System;
using Snappy;

namespace IronLeveldb.Snappy
{
    public class SnappyNet : ISnappyDecompressor
    {
        public byte[] Decompress(byte[] src, int srcPos, int srcSize)
        {
            var uncompressedLength = SnappyCodec.GetUncompressedLength(src, srcPos, srcSize);
            var output = new byte[uncompressedLength];
            var length = SnappyCodec.Uncompress(src, srcPos, srcSize, output, 0);
            if (length == uncompressedLength)
            {
                return output;
            }

            Array.Resize(ref output, length);
            return output;
        }
    }
}
#endif
