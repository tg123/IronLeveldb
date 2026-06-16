using IronLeveldb.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronLeveldb.Test.Storage
{
    [TestClass]
    public class FileNameMakerTests
    {
        [TestMethod]
        public void CurrentFileName()
        {
            Assert.AreEqual("CURRENT", FileNameMaker.CurrentFileName());
        }

        [TestMethod]
        public void DescriptorFileName()
        {
            Assert.AreEqual("MANIFEST-000000", FileNameMaker.DescriptorFileName(0));
            Assert.AreEqual("MANIFEST-000042", FileNameMaker.DescriptorFileName(42));
            Assert.AreEqual("MANIFEST-123456", FileNameMaker.DescriptorFileName(123456));
        }

        [TestMethod]
        public void TableFileName()
        {
            Assert.AreEqual("000000.ldb", FileNameMaker.TableFileName(0));
            Assert.AreEqual("000007.ldb", FileNameMaker.TableFileName(7));
            Assert.AreEqual("000100.ldb", FileNameMaker.TableFileName(100));
        }
    }
}
