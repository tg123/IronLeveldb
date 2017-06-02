using System.Text;

namespace IronLevelDB
{
    public interface IByteArrayKeyValuePair : IKeyValuePair<byte[], byte[]>
    {
    }

    public static class ByteArrayKeyValuePairExt
    {
        public static string KeyAsString(this IByteArrayKeyValuePair kv)
        {
            return Encoding.ASCII.GetString(kv.Key, 0, kv.Key.Length);
        }

        public static string ValueAsString(this IByteArrayKeyValuePair kv)
        {
            return Encoding.ASCII.GetString(kv.Value);
        }
    }
}