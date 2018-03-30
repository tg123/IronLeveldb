using System.Collections.Generic;

namespace IronLevelDB
{
    public interface IKeyValuePair<out TK, out TV>
    {
        TK Key { get; }
        TV Value { get; }
    }

    public static class KeyValuePairExt
    {
        public static KeyValuePair<TK, TV> ToKeyValuePair<TK, TV>(this IKeyValuePair<TK, TV> kv)
        {
            return new KeyValuePair<TK, TV>(kv.Key, kv.Value);
        }
    }
}
