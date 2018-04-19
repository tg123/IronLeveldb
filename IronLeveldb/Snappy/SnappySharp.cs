using Snappy.Sharp;

namespace IronLeveldb.Snappy
{
    public class SnappySharp : ISnappyDecompressor
    {
        private readonly SnappyDecompressor _decompressor = new SnappyDecompressor();

        public byte[] Decompress(byte[] src, int srcPos, int srcSize)
        {
            return _decompressor.Decompress(src, srcPos, srcSize);
        }
    }
}
