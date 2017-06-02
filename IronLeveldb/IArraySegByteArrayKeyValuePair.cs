using System;

namespace IronLevelDB
{
    internal interface IArraySegByteArrayKeyValuePair : IKeyValuePair<ArraySegment<byte>, ArraySegment<byte>>
    {
    }
}