using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronLevelDB.Test
{
    public static class IronLeveldbTestExt
    {
        public static string GetAsString(this IIronLeveldb db, string key)
        {
            return BytesToString(db.Get(key));
        }

        public static byte[] Get(this IIronLeveldb db, string key)
        {
            return db.Get(StringToBytes(key))?.ToArray();
        }

        public static IEnumerable<IReadonlyBytesKeyValuePair> Seek(this IIronLeveldb db, string key)
        {
            return db.Seek(StringToBytes(key));
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
    }
}