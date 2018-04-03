using System.Collections.Generic;

namespace IronLevelDB
{
    public interface IKeyComparer : IComparer<IReadOnlyList<byte>>
    {
        // TODO Name
        // string Name { get; }
    }
}
