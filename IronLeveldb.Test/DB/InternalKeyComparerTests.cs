using IronLevelDB.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronLevelDB.Test.DB
{
    [TestClass]
    public class InternalKeyComparerTests
    {
        [TestMethod]
        public void CompareTest()
        {
            var comparer = new InternalKeyComparer(LeveldbDefaultKeyComparer.Comparer);

            Assert.IsTrue(comparer.Compare(
                              new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Value)
                              , new InternalKey(new byte[] {2}, 0, InternalKey.ValueType.Value)
                          ) < 0);
            Assert.IsTrue(comparer.Compare(
                              new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Value)
                              , new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Value)
                          ) == 0);
            Assert.IsTrue(comparer.Compare(
                              new InternalKey(new byte[] {2}, 0, InternalKey.ValueType.Value)
                              , new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Value)
                          ) > 0);

            Assert.IsTrue(comparer.Compare(
                              new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Value)
                              , new InternalKey(new byte[] {1, 1}, 0, InternalKey.ValueType.Value)
                          ) < 0);
            Assert.IsTrue(comparer.Compare(
                              new InternalKey(new byte[] {1, 1}, 0, InternalKey.ValueType.Value)
                              , new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Value)
                          ) > 0);


            Assert.IsTrue(comparer.Compare(
                              new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Value)
                              , new InternalKey(new byte[] {1}, 1, InternalKey.ValueType.Value)
                          ) > 0);

            Assert.IsTrue(comparer.Compare(
                              new InternalKey(new byte[] {1}, 1, InternalKey.ValueType.Value)
                              , new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Value)
                          ) < 0);


            Assert.IsTrue(comparer.Compare(
                              new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Deletion)
                              , new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Value)
                          ) > 0);

            Assert.IsTrue(comparer.Compare(
                              new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Value)
                              , new InternalKey(new byte[] {1}, 0, InternalKey.ValueType.Deletion)
                          ) < 0);
        }
    }
}