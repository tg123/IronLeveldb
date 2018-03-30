using System.Collections.Concurrent;
using System.Collections.Generic;

namespace IronLevelDB.Cache
{
    public class PermanentCache : ICache
    {
        private readonly ConcurrentDictionary<ByteArrayKey, object> _data =
            new ConcurrentDictionary<ByteArrayKey, object>();

        public void Insert<T>(long namespaceId, IReadOnlyList<byte> key, T value)
        {
            _data.AddOrUpdate(new ByteArrayKey(namespaceId, key), value, (_, old) => value);
        }

        public T Lookup<T>(long namespaceId, IReadOnlyList<byte> key)
        {
            if (!_data.TryGetValue(new ByteArrayKey(namespaceId, key), out var v))
            {
                return default(T);
            }

            return v is T ? (T) v : default(T);
        }

        public void Erase(long namespaceId, IReadOnlyList<byte> key)
        {
            _data.TryRemove(new ByteArrayKey(namespaceId, key), out object _);
        }

        public void Prune()
        {
            _data.Clear();
        }
    }
}
