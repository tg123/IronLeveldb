using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronLevelDB.Test
{
    [TestClass]
    public class LeveldbDefaultKeyComparerTests
    {
        [TestMethod]
        public void CompareTest()
        {
            var comparer = LeveldbDefaultKeyComparer.Comparer;

            Assert.IsTrue(comparer.Compare(new byte[] {1}, new byte[] {2}) < 0);
            Assert.IsTrue(comparer.Compare(new byte[] {1}, new byte[] {1}) == 0);
            Assert.IsTrue(comparer.Compare(new byte[] {2}, new byte[] {1}) > 0);

            Assert.IsTrue(comparer.Compare(new byte[] {1}, new byte[] {1, 1}) < 0);
            Assert.IsTrue(comparer.Compare(new byte[] {1, 1}, new byte[] {1}) > 0);
        }
    }
}
