using System.Collections.Generic;

namespace IronLevelDB
{
    public interface IReadonlyBytesKeyValuePair : IKeyValuePair<IReadOnlyList<byte>, IReadOnlyList<byte>>
    {
    }
}