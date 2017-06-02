using System;
using System.Diagnostics;
using System.Linq;

namespace IronLevelDB.DB
{
    internal class InternalKey
    {
        public enum ValueType
        {
            Deletion = 0x0,
            Value = 0x1
        }

        private readonly Lazy<InternalKeyValue> _internalkey;

        internal InternalKey(ArraySegment<byte> key)
        {
            _internalkey = new Lazy<InternalKeyValue>(() =>
            {
                var num = BitConverter.ToUInt64(key.Array, key.Offset + key.Count - 8);

                return new InternalKeyValue
                {
                    UserKey = ExtractUserKey(key).ToArray(),
                    Sequence = num >> 8,
                    Type = (ValueType) (num & 0xff)
                };
            });
        }

        internal InternalKey(byte[] userkey, ulong sequence, ValueType type)
        {
            _internalkey = new Lazy<InternalKeyValue>(() => new InternalKeyValue
            {
                UserKey = userkey,
                Sequence = sequence,
                Type = type
            });
        }

        public byte[] UserKey => _internalkey.Value.UserKey;
        public ulong Sequence => _internalkey.Value.Sequence;
        public ValueType Type => _internalkey.Value.Type;

        private ArraySegment<byte> ExtractUserKey(ArraySegment<byte> key)
        {
            Debug.Assert(key.Count > 8);
            return new ArraySegment<byte>(key.Array, key.Offset, key.Count - 8);
        }

        private struct InternalKeyValue
        {
            public ulong Sequence;
            public byte[] UserKey;
            public ValueType Type;
        }
    }
}