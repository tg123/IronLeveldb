using IronLeveldb.Cache.LRU;

namespace IronLeveldb
{
    public class DefaultIronLeveldbOptions : IIronLeveldbOptions
    {
        public IKeyComparer Comparer { get; set; } = LeveldbDefaultKeyComparer.Comparer;

        // TODO better default cache
        public ICache TableCache { get; set; } = new LruCache(2 * 1024 * 1024 * 100); // max_openfile 100

        public ICache BlockCache { get; set; } = new LruCache(8 * 1024 * 1024);
    }
}
