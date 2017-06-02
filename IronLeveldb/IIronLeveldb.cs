using System;

namespace IronLevelDB
{
    public interface IIronLeveldb : ISeekable<byte[], IByteArrayKeyValuePair>, IDisposable
    {
        // TODO Put
    }
}