using System;
using IronLevelDB.SSTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronLevelDB.Test.SSTable
{
    [TestClass]
    public class AppendableByteArraySegmentTests
    {
        [TestMethod]
        public void AppendTest()
        {
            {
                var str = new AppendableByteArraySegment(10);

                str.Append("1");
                str.Append("2");
                str.Append("3");

                Assert.AreEqual("123", str.GetString());
            }

            {
                var str = new AppendableByteArraySegment(10);

                str.Append("10000");
                str.Append("20000");
                str.Append("30000");

                Assert.AreEqual("100002000030000", str.GetString());
            }

            {
                do
                {
                    try
                    {
                        var str = new AppendableByteArraySegment(1);

                        str.Append("10000");
                        str.Append("20000");
                    }
                    catch (OverflowException)
                    {
                        break;
                    }

                    Assert.Fail();
                } while (false);
            }
        }

        [TestMethod]
        public void TrimToLengthTest()
        {
//            {
//                var str = new AppendableByteArraySegment(10);
//
//                str.Append("1");
//                str.Append("2");
//                str.Append("3");
//
//                str.TrimToLength(2);
//                Assert.AreEqual("12", str.GetString());
//
//                str.TrimToLength(1);
//                Assert.AreEqual("1", str.GetString());
//
//                str.Append("abc");
//                Assert.AreEqual("1abc", str.GetString());
//
//                str.TrimToLength(0);
//                Assert.AreEqual("", str.GetString());
//            }

            {
                var str = new AppendableByteArraySegment(10);

                str.Append("12345");
                str.Append("67890");

                Assert.AreEqual("1234567890", str.GetString());

                str.TrimToLength(9);
                Assert.AreEqual("123456789", str.GetString());

                str.TrimToLength(8);
                Assert.AreEqual("12345678", str.GetString());

                str.TrimToLength(3);
                Assert.AreEqual("123", str.GetString());
            }
        }

        [TestMethod]
        public void TrimClone()
        {
            {
                var str1 = new AppendableByteArraySegment(10);

                str1.Append("12345");
                str1.Append("67890");

                var str2 = str1.Clone();

                str2.TrimToLength(1);

                Assert.AreEqual("1234567890", str1.GetString());
                Assert.AreEqual("1", str2.GetString());
            }
        }
    }
}
