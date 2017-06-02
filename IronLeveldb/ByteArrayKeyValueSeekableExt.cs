using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronLevelDB
{
    public static class ByteArrayKeyValueSeekableExt
    {
        public static IEnumerable<TV> Seek<TV>(this ISeekable<byte[], TV> seekable, string key)
        {
            return seekable.Seek(StringToBytes(key));
        }

        private static byte[] StringToBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private static string BytesToString(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(bytes);
        }

        public static byte[] Get(this ISeekable<byte[], IByteArrayKeyValuePair> seekable, byte[] key)
        {
            var v = seekable.Seek(key).FirstOrDefault();

            var sk = v?.Key;

            if (sk?.Length == key.Length)
            {
                for (var i = key.Length - 1; i >= 0; i--)
                {
                    if (key[i] != sk[i])
                    {
                        return null;
                    }
                }

                return v.Value;
            }

            return null;
        }

        public static byte[] Get(this ISeekable<byte[], IByteArrayKeyValuePair> seekable, string key)
        {
            return seekable.Get(StringToBytes(key));
        }

        public static string GetAsString(this ISeekable<byte[], IByteArrayKeyValuePair> seekable, string key)
        {
            return BytesToString(seekable.Get(StringToBytes(key)));
        }
    }
}