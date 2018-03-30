using System;
using System.Collections.Generic;
using IronLevelDB.SSTable;

namespace IronLevelDB.DB
{
    internal class InternalIByteArrayKeyValuePair : IReadonlyBytesKeyValuePair
    {
        private readonly ArraySegment<byte> _value;

        public InternalIByteArrayKeyValuePair(AppendableByteArraySegment key,
            ArraySegment<byte> value)
        {
            InternalKey = new InternalKey(key);
            _value = value;
        }

        public InternalKey InternalKey { get; }

        public IReadOnlyList<byte> Value => _value;

        public IReadOnlyList<byte> Key => InternalKey.UserKey;
    }
}
