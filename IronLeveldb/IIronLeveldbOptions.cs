namespace IronLevelDB
{
    public interface IIronLeveldbOptions
    {
        IKeyComparer Comparer { get; }
        IIronLeveldbStorge Storge { get; }

        ICache TableCache { get; }

        ICache BlockCache { get; }

        // TODO sync up with native leveldb
    }
}
