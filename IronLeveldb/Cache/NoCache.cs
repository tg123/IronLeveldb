using System.Collections.Generic;

namespace IronLeveldb.Cache
{
    public class NoCache : ICache
    {
        public void Prune()
        {
        }

        public void Insert<T>(long namespaceId, IReadOnlyList<byte> key, T value)
        {
        }

        public T Lookup<T>(long namespaceId, IReadOnlyList<byte> key)
        {
            return default(T);
        }

        public void Erase(long namespaceId, IReadOnlyList<byte> key)
        {
        }
    }
}
