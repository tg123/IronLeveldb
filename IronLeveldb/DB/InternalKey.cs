using System;
using System.Collections.Generic;
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

        internal InternalKey(IEnumerable<byte> rawkey) : this(new ArraySegment<byte>(rawkey.ToArray()))
        {
        }

        internal InternalKey(ArraySegment<byte> rawkey)
        {
            var num = BitConverter.ToUInt64(rawkey.Array, rawkey.Offset + rawkey.Count - 8);
            _internalkey = new InternalKeyValue
            {
                UserKey = ExtractUserKey(rawkey),
                Sequence = num >> 8,
                Type = (ValueType) (num & 0xff)
            };
        }

        internal InternalKey(IReadOnlyList<byte> userkey, ulong sequence, ValueType type)
        {
            _internalkey = new InternalKeyValue
            {
                UserKey = userkey,
                Sequence = sequence,
                Type = type
            };
        }

        public IReadOnlyList<byte> UserKey => _internalkey.UserKey;
        public ulong Sequence => _internalkey.Sequence;
        public ValueType Type => _internalkey.Type;

//        public byte[] ToByteArray()
//        {
//            var len = _internalkey.UserKey.Length;
//            var bytes = new byte[len + 8];
//            Buffer.BlockCopy(_internalkey.UserKey, 0, bytes, 0, len);
//
//            Buffer.BlockCopy(BitConverter.GetBytes(PackSequenceAndType(_internalkey.Sequence, _internalkey.Type)), 0,
//                bytes, len, 8);
//
//            return bytes;
//        }

        private static ArraySegment<byte> ExtractUserKey(ArraySegment<byte> key)
        {
            Debug.Assert(key.Count > 8);
            return new ArraySegment<byte>(key.Array, key.Offset, key.Count - 8);
        }

//        private static ulong PackSequenceAndType(ulong seq, ValueType t)
//        {
//            return (seq << 8) | (ulong) t;
//        }

        private struct InternalKeyValue
        {
            public ulong Sequence;
            public IReadOnlyList<byte> UserKey;
            public ValueType Type;
        }
    }
}
