using System.Linq;
using IronLevelDB.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronLevelDB.Test.DB
{
    [TestClass]
    public class MergingEnumerableTests
    {
        [TestMethod]
        public void MergingEnumerableSingleInput()
        {
            var one = new[] {1, 2, 3};
            var merged = new MergingEnumerable<int>(new[] {one});
            Assert.IsTrue(one.SequenceEqual(merged));
        }

        [TestMethod]
        public void MergingEnumerableEmpty()
        {
            var one = new[] {1, 2, 3};
            var merged = new MergingEnumerable<int>(new[] {one, new int[0]});
            Assert.IsTrue(one.SequenceEqual(merged));
        }

        [TestMethod]
        public void MergingEnumerableConcat()
        {
            var one = new[] {1, 2, 3};
            var two = new[] {4, 5, 6};

            var merged = new MergingEnumerable<int>(new[] {one, two});
            Assert.IsTrue(one.Concat(two).SequenceEqual(merged));
        }

        [TestMethod]
        public void MergingEnumerableConcatAndOrderBy()
        {
            var one = new[] {1, 3, 5};
            var two = new[] {2, 4, 6};

            var merged = new MergingEnumerable<int>(new[] {one, two});
            Assert.IsTrue(one.Concat(two).OrderBy(x => x).SequenceEqual(merged));
        }
    }
}
