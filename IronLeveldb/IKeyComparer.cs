using System.Collections.Generic;

namespace IronLevelDB
{
    public interface IKeyComparer : IComparer<byte[]>
    {
        // TODO Name
        // string Name { get; }
    }
}