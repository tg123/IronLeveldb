using System;
using System.Collections.Generic;
using System.Linq;

namespace IronLevelDB.DB
{
    internal static class InternalKeyExt
    {
        internal static IEnumerable<InternalIByteArrayKeyValuePair> FilterDeleted(
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
    }
}