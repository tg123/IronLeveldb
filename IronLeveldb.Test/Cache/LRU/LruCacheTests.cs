using System;
using IronLevelDB.Cache.LRU;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronLevelDB.Test.Cache.LRU
{
    internal static class CacheExt
    {
        internal static void Insert(this ICache cache, int key, int? value)
        {
            cache.Insert(0, BitConverter.GetBytes(key), value);
        }

        internal static int? Lookup(this ICache cache, int key)
        {
            return cache.Lookup<int?>(0, BitConverter.GetBytes(key));
        }
    }

    [TestClass]
    public class LruCacheTests
    {
        [TestMethod]
        public void LeetcodeTest()
        {
            // this case copy from leetcode
            var cache = new LruCache(2 /* capacity */);

            cache.Insert(1, 1);
            cache.Insert(2, 2);

            Assert.AreEqual(1, cache.Lookup(1)); // returns 1

            cache.Insert(3, 3); // evicts key 2
            Assert.AreEqual(null, cache.Lookup(2)); // returns null (not found)
            cache.Insert(4, 4); // evicts key 1
            Assert.AreEqual(null, cache.Lookup(1)); // returns null (not found)
            Assert.AreEqual(3, cache.Lookup(3)); // returns 3
            Assert.AreEqual(4, cache.Lookup(4)); // returns 4
        }

//        [TestMethod]
//        public void LookupTest()
//        {
//            Assert.Fail();
//        }
//
//        [TestMethod]
//        public void EraseTest()
//        {
//            Assert.Fail();
//        }
//
//        [TestMethod]
//        public void PruneTest()
//        {
//            Assert.Fail();
//        }
    }
}
