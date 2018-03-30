namespace IronLevelDB
{
    public interface IContentReader
    {
        long ContentLength { get; }

        byte[] ReadContent(long offset, long size);
    }
}
