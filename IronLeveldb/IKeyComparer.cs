using System.Collections.Generic;

namespace IronLeveldb
{
    public interface IKeyComparer : IComparer<IReadOnlyList<byte>>
    {
        // TODO Name
        // string Name { get; }
    }
}
