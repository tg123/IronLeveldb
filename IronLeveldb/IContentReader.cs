namespace IronLeveldb
{
    public interface IContentReader
    {
        long ContentLength { get; }

        byte[] ReadContent(long offset, long size);
    }
}
