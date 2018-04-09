using System.Collections.Generic;

namespace IronLeveldb
{
    public interface IReadonlyBytesKeyValuePair : IKeyValuePair<IReadOnlyList<byte>, IReadOnlyList<byte>>
    {
    }
}
