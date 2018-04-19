namespace IronLeveldb
{
    public interface ISnappyDecompressor
    {
        byte[] Decompress(byte[] src, int srcPos, int srcSize);
    }

    public static class SnappyDecompressorExt
    {
        public static byte[] Decompress(this ISnappyDecompressor decompressor, byte[] src)
        {
            return decompressor.Decompress(src, 0, src.Length);
        }
    }
}
