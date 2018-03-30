using System.Collections.Concurrent;

namespace IronLevelDB.Cache
{
    public class PermanentCache : ICache
    {
        private readonly ConcurrentDictionary<ByteArrayKey, object> _data =
            new ConcurrentDictionary<ByteArrayKey, object>();

        public void Insert<T>(byte[] key, T value)
        {
            _data.AddOrUpdate(new ByteArrayKey(key), value, (_, old) => value);
        }

        public T Lookup<T>(byte[] key)
        {
            object v;
            if (!_data.TryGetValue(new ByteArrayKey(key), out v))
            {
                return default(T);
            }

            return v is T ? (T) v : default(T);
        }

        public void Erase(byte[] key)
        {
            object _;
            _data.TryRemove(new ByteArrayKey(key), out _);
        }

        public void Prune()
        {
            _data.Clear();
        }

        public long NewId()
        {
            return IdGenerator.NewId();
        }
    }
}
