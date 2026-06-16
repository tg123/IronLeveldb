using IronLeveldb.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronLeveldb.Test.Cache
{
    [TestClass]
    public class NoCacheTests
    {
        [TestMethod]
        public void LookupAlwaysReturnsDefault()
        {
            var cache = new NoCache();
            cache.Insert<int?>(0, new byte[] {1, 2, 3}, 42);

            Assert.IsNull(cache.Lookup<int?>(0, new byte[] {1, 2, 3}));

            // erase and prune must be no-ops and must not throw
            cache.Erase(0, new byte[] {1, 2, 3});
            cache.Prune();
        }
    }

    [TestClass]
    public class PermanentCacheTests
    {
        [TestMethod]
        public void InsertLookup()
        {
            var cache = new PermanentCache();

            cache.Insert(1, new byte[] {1, 2, 3}, "hello");

            Assert.AreEqual("hello", cache.Lookup<string>(1, new byte[] {1, 2, 3}));

            // a copy of the same key bytes must hit the same entry
            Assert.AreEqual("hello", cache.Lookup<string>(1, new byte[] {1, 2, 3}));
        }

        [TestMethod]
        public void NamespaceIsolation()
        {
            var cache = new PermanentCache();

            cache.Insert(1, new byte[] {1}, "ns1");
            cache.Insert(2, new byte[] {1}, "ns2");

            Assert.AreEqual("ns1", cache.Lookup<string>(1, new byte[] {1}));
            Assert.AreEqual("ns2", cache.Lookup<string>(2, new byte[] {1}));
        }

        [TestMethod]
        public void Overwrite()
        {
            var cache = new PermanentCache();

            cache.Insert(0, new byte[] {9}, "old");
            cache.Insert(0, new byte[] {9}, "new");

            Assert.AreEqual("new", cache.Lookup<string>(0, new byte[] {9}));
        }

        [TestMethod]
        public void MissReturnsDefault()
        {
            var cache = new PermanentCache();

            Assert.IsNull(cache.Lookup<string>(0, new byte[] {1}));
            Assert.AreEqual(0, cache.Lookup<int>(0, new byte[] {1}));
        }

        [TestMethod]
        public void WrongTypeReturnsDefault()
        {
            var cache = new PermanentCache();

            cache.Insert(0, new byte[] {1}, "a string");

            Assert.AreEqual(0, cache.Lookup<int>(0, new byte[] {1}));
        }

        [TestMethod]
        public void Erase()
        {
            var cache = new PermanentCache();

            cache.Insert(0, new byte[] {1}, "value");
            cache.Erase(0, new byte[] {1});

            Assert.IsNull(cache.Lookup<string>(0, new byte[] {1}));
        }

        [TestMethod]
        public void Prune()
        {
            var cache = new PermanentCache();

            cache.Insert(0, new byte[] {1}, "a");
            cache.Insert(0, new byte[] {2}, "b");

            cache.Prune();

            Assert.IsNull(cache.Lookup<string>(0, new byte[] {1}));
            Assert.IsNull(cache.Lookup<string>(0, new byte[] {2}));
        }
    }

    [TestClass]
    public class IdGeneratorTests
    {
        [TestMethod]
        public void NewIdIsMonotonicAndUnique()
        {
            var first = IdGenerator.NewId();
            var second = IdGenerator.NewId();

            Assert.IsTrue(second > first);
        }
    }
}
