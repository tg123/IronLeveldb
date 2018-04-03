using System.Collections.Generic;

namespace IronLevelDB
{
    public interface ICache
    {
        void Insert<T>(long namespaceId, IReadOnlyList<byte> key, T value);

        T Lookup<T>(long namespaceId, IReadOnlyList<byte> key);

        void Erase(long namespaceId, IReadOnlyList<byte> key);

        void Prune();
    }
}
