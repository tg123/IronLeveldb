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

        public const ulong MaxSequenceNumber = (0x1ul << 56) - 1;

        public const ValueType ValueTypeForSeek = ValueType.Value;

        private readonly InternalKeyValue _internalkey;

        internal InternalKey(ArraySegment<byte> key)
        {
            var num = BitConverter.ToUInt64(key.Array, key.Offset + key.Count - 8);
            _internalkey = new InternalKeyValue
            {
                UserKey = ExtractUserKey(key).ToArray(),
                Sequence = num >> 8,
                Type = (ValueType) (num & 0xff)
            };
        }

        internal InternalKey(byte[] userkey, ulong sequence, ValueType type)
        {
            _internalkey = new InternalKeyValue
            {
                UserKey = userkey,
                Sequence = sequence,
                Type = type
            };
        }

        public byte[] UserKey => _internalkey.UserKey;
        public ulong Sequence => _internalkey.Sequence;
        public ValueType Type => _internalkey.Type;

        public byte[] ToByteArray()
        {
            var len = _internalkey.UserKey.Length;
            var bytes = new byte[len + 8];
            Buffer.BlockCopy(_internalkey.UserKey, 0, bytes, 0, len);

            Buffer.BlockCopy(BitConverter.GetBytes(PackSequenceAndType(_internalkey.Sequence, _internalkey.Type)), 0,
                bytes, len, 8);

            return bytes;
        }

        private ArraySegment<byte> ExtractUserKey(ArraySegment<byte> key)
        {
            Debug.Assert(key.Count > 8);
            return new ArraySegment<byte>(key.Array, key.Offset, key.Count - 8);
        }

        private ulong PackSequenceAndType(ulong seq, ValueType t)
        {
            return (seq << 8) | (ulong) t;
        }

        private struct InternalKeyValue
        {
            public ulong Sequence;
            public byte[] UserKey;
            public ValueType Type;
        }
    }
}
