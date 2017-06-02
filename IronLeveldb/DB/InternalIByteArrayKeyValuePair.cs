using System;

namespace IronLevelDB.DB
{
    internal class InternalIByteArrayKeyValuePair : IByteArrayKeyValuePair
    {
        private readonly IByteArrayKeyValuePair _real;

        public InternalIByteArrayKeyValuePair(IByteArrayKeyValuePair real)
        {
            var key = (real as IArraySegByteArrayKeyValuePair)?.Key ?? new ArraySegment<byte>(real.Key);

            InternalKey = new InternalKey(key);
            _real = real;
        }

        public InternalKey InternalKey { get; }

        public byte[] Key => InternalKey.UserKey;

        public byte[] Value => _real.Value;
    }
}