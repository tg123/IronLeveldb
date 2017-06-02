using System;

namespace IronLevelDB
{
    public interface ICache
    {
        // TODO charge or not?
        void Insert<T>(byte[] key, T value);

        T Lookup<T>(byte[] key);

        void Erase(byte[] key);

        void Prune();

        long NewId();
    }

    public static class CacheExt
    {
        public static void Insert<T>(this ICache cache, long id, byte[] key, T value)
        {
            cache.Insert(KeyWithNamespace(id, key), value);
        }

        public static T Lookup<T>(this ICache cache, long id, byte[] key)
        {
            return cache.Lookup<T>(KeyWithNamespace(id, key));
        }


        public static void Erase(this ICache cache, long id, byte[] key)
        {
            cache.Erase(KeyWithNamespace(id, key));
        }


        private static byte[] KeyWithNamespace(long id, byte[] key)
        {
            var newkey = BitConverter.GetBytes(id);
            Array.Resize(ref newkey, sizeof(long) + key.Length);
            Array.Copy(key, 0, newkey, sizeof(long), key.Length);
            return newkey;
        }
    }
}