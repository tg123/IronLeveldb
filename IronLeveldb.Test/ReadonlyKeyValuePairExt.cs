using System.Linq;
using System.Text;

namespace IronLevelDB.Test
{
    internal static class ReadonlyKeyValuePairExt
    {
        public static string KeyAsString(this IReadonlyBytesKeyValuePair kv)
        {
            return Encoding.ASCII.GetString(kv.Key.ToArray(), 0, kv.Key.Count);
        }

        public static string ValueAsString(this IReadonlyBytesKeyValuePair kv)
        {
            return Encoding.ASCII.GetString(kv.Value.ToArray());
        }
    }
}