using IronLeveldb.Cache.LRU;
using IronLeveldb.DB;
using IronLeveldb.Snappy;

namespace IronLeveldb
{
    public class IronLeveldbOptions
    {
        private IKeyComparer _comparer;

        public IronLeveldbOptions()
        {
            Comparer = LeveldbDefaultKeyComparer.Comparer;
        }

        internal InternalKeyComparer InternalKeyComparer { get; private set; }

        public IKeyComparer Comparer
        {
            get => _comparer;
            set
            {
                _comparer = value;
                InternalKeyComparer = new InternalKeyComparer(_comparer);
            }
        }

        // TODO better default cache
        public ICache TableCache { get; set; } = new LruCache(2 * 1024 * 1024 * 100); // max_openfile 100

        public ICache BlockCache { get; set; } = new LruCache(8 * 1024 * 1024);

        public ISnappyDecompressor SnappyDecompressor { get; set; }
#if NET45
            = new SnappyNet();
#else
            = new SnappySharp();
#endif

        public bool ParanoidChecks { get; set; } = false;
    }
}
