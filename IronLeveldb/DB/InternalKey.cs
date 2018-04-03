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

        internal InternalKey(IEnumerable<byte> rawkey) : this(new ArraySegment<byte>(rawkey.ToArray()))
        {
        }

        internal InternalKey(ArraySegment<byte> rawkey)
        {
            var num = BitConverter.ToUInt64(rawkey.Array, rawkey.Offset + rawkey.Count - 8);
            UserKey = ExtractUserKey(rawkey);
            Sequence = num >> 8;
            Type = (ValueType) (num & 0xff);
        }

        internal InternalKey(IReadOnlyList<byte> userkey, ulong sequence, ValueType type)
        {
            UserKey = userkey;
            Sequence = sequence;
            Type = type;
        }

        public IReadOnlyList<byte> UserKey { get; }
        public ulong Sequence { get; }
        public ValueType Type { get; }

        internal static InternalKey FromUnsafe(byte[] rawkey)
        {
            return new InternalKey(new ArraySegment<byte>(rawkey));
        }

        private static ArraySegment<byte> ExtractUserKey(ArraySegment<byte> key)
        {
            Debug.Assert(key.Count > 8);
            return new ArraySegment<byte>(key.Array, key.Offset, key.Count - 8);
        }
    }
}
