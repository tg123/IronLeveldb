namespace IronLeveldb
{
    public interface IIronLeveldbOptions
    {
        IKeyComparer Comparer { get; }

        ICache TableCache { get; }

        ICache BlockCache { get; }

        // TODO sync up with native leveldb
    }
}
