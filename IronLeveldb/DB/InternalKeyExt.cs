using System;
using System.Collections.Generic;
using System.Linq;

namespace IronLevelDB.DB
{
    internal static class InternalKeyExt
    {
        public static IEnumerable<IByteArrayKeyValuePair> FilterDeleted(
            this IEnumerable<InternalIByteArrayKeyValuePair> internalkvs, IKeyComparer comparer)
        {
            byte[] lastkey = null;

            foreach (var kv in internalkvs)
            {
                switch (kv.InternalKey.Type)
                {
                    case InternalKey.ValueType.Deletion:
                        lastkey = kv.Key;

                        break;
                    case InternalKey.ValueType.Value:
                        if (lastkey != null && comparer.Compare(kv.Key, lastkey) <= 0)
                        {
                            // skip
                        }
                        else
                        {
                            yield return kv;
                            lastkey = kv.Key;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static IEnumerable<InternalIByteArrayKeyValuePair> ToInternalKeyValuePairs(
            this IEnumerable<IByteArrayKeyValuePair> rawKeyValuePairs)
        {
            return rawKeyValuePairs.Select(kv => new InternalIByteArrayKeyValuePair(kv));
        }

        public static ISeekable<byte[], InternalIByteArrayKeyValuePair> ToInternalKeyValuePairs(
            this ISeekable<byte[], IByteArrayKeyValuePair> rawKeyValuePairs)
        {
            return new InternalSeekableWrapper(rawKeyValuePairs);
        }

        private class InternalSeekableWrapper : ISeekable<byte[], InternalIByteArrayKeyValuePair>
        {
            private readonly ISeekable<byte[], IByteArrayKeyValuePair> _raw;

            public InternalSeekableWrapper(ISeekable<byte[], IByteArrayKeyValuePair> raw)
            {
                _raw = raw;
            }

            public IEnumerable<InternalIByteArrayKeyValuePair> Seek(byte[] key)
            {
                return _raw.Seek(key).ToInternalKeyValuePairs();
            }

            public IEnumerable<InternalIByteArrayKeyValuePair> SeekFirst()
            {
                return _raw.SeekFirst().ToInternalKeyValuePairs();
            }
        }
    }
}