using System.IO;
using System.Linq;
using LevelDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronLevelDB.Test
{
    [TestClass]
    public class ReadonlyFileSystemLeveldbTests
    {
        [TestMethod]
        public void TestReadNew()
        {
            var dbpath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dbpath);

            try
            {
                using (var db = LevelDB.DB.Open(dbpath, new Options
                {
                    CreateIfMissing = true,
                    WriteBufferSize = ulong.MaxValue
                }))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        db.Put(new WriteOptions(), "k" + i, "v" + i);
                    }
                }

                // force leveldb to flush and recover log
                using (LevelDB.DB.Open(dbpath, new Options()))
                {
                }

                using (var db = IronLeveldbBuilder.BuildFromPath(dbpath))
                {
                    Assert.AreEqual("v9", db.GetAsString("k9"));

                    Assert.IsTrue(db.SeekFirst()
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(Enumerable.Range(0, 10).Select(i => $"k{i} - v{i}"))
                    );

                    for (var i = 0; i < 10; i++)
                    {
                        Assert.AreEqual("v" + i, db.GetAsString("k" + i));
                    }

                    Assert.IsTrue(db.Seek("k5")
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(Enumerable.Range(5, 5).Select(i => $"k{i} - v{i}"))
                    );
                    Assert.AreEqual(null, db.Get("not exists"));
                }
            }
            finally
            {
                Directory.Delete(dbpath, true);
            }
        }

        [TestMethod]
        public void TestReadOverwrite()
        {
            var dbpath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dbpath);

            try
            {
                using (var db = LevelDB.DB.Open(dbpath, new Options
                {
                    CreateIfMissing = true,
                    WriteBufferSize = ulong.MaxValue
                }))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        db.Put(new WriteOptions(), "k" + i, "v" + i);
                    }
                }

                using (var db = LevelDB.DB.Open(dbpath, new Options()))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        db.Put(new WriteOptions(), "k" + i, "vv" + i);
                    }
                }

                // force leveldb to flush and recover log
                using (LevelDB.DB.Open(dbpath, new Options()))
                {
                }

                using (var db = IronLeveldbBuilder.BuildFromPath(dbpath))
                {
                    Assert.IsTrue(db.SeekFirst()
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(Enumerable.Range(0, 10).Select(i => $"k{i} - vv{i}"))
                    );

                    for (var i = 0; i < 10; i++)
                    {
                        Assert.AreEqual("vv" + i, db.GetAsString("k" + i));
                    }

                    Assert.IsTrue(db.Seek("k5")
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(Enumerable.Range(5, 5).Select(i => $"k{i} - vv{i}"))
                    );

                    Assert.AreEqual(null, db.Get("not exists"));
                }
            }
            finally
            {
                Directory.Delete(dbpath, true);
            }
        }

        [TestMethod]
        public void TestReadDeleted()
        {
            var dbpath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dbpath);

            try
            {
                using (var db = LevelDB.DB.Open(dbpath, new Options
                {
                    CreateIfMissing = true,
                    WriteBufferSize = ulong.MaxValue
                }))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        db.Put(new WriteOptions(), "k" + i, "v" + i);
                    }
                }

                using (var db = LevelDB.DB.Open(dbpath, new Options()))
                {
                    for (var i = 5; i < 10; i++)
                    {
                        db.Delete(new WriteOptions(), "k" + i);
                    }
                }

                // force leveldb to flush
                using (LevelDB.DB.Open(dbpath, new Options()))
                {
                }

                using (var db = IronLeveldbBuilder.BuildFromPath(dbpath))
                {
                    Assert.IsTrue(db.SeekFirst()
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(Enumerable.Range(0, 5).Select(i => $"k{i} - v{i}"))
                    );

                    for (var i = 0; i < 5; i++)
                    {
                        Assert.AreEqual("v" + i, db.GetAsString("k" + i));
                    }

                    for (var i = 5; i < 10; i++)
                    {
                        Assert.AreEqual(null, db.GetAsString("k" + i));
                    }

                    Assert.IsTrue(db.Seek("k3")
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(Enumerable.Range(3, 2).Select(i => $"k{i} - v{i}"))
                    );

                    Assert.AreEqual(null, db.Get("not exists"));
                    Assert.AreEqual(null, db.Get("k6"));

                    Assert.IsTrue(db.Seek("a").Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(db.Seek("b").Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}"))
                    );

                    // not found
                    Assert.IsTrue(db.Seek("xxx").Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(db.Seek("yyy").Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}"))
                    );
                }


                // delete again
                using (var db = LevelDB.DB.Open(dbpath, new Options()))
                {
                    for (var i = 0; i < 5; i++)
                    {
                        db.Delete(new WriteOptions(), "k" + i);
                    }
                }

                using (var db = LevelDB.DB.Open(dbpath, new Options()))
                {
                    for (var i = 5; i < 10; i++)
                    {
                        db.Put(new WriteOptions(), "k" + i, "v" + i);
                    }
                }

                // force leveldb to flush
                using (LevelDB.DB.Open(dbpath, new Options()))
                {
                }


                using (var db = IronLeveldbBuilder.BuildFromPath(dbpath))
                {
                    Assert.IsTrue(db.SeekFirst()
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(Enumerable.Range(5, 5).Select(i => $"k{i} - v{i}"))
                    );

                    for (var i = 0; i < 5; i++)
                    {
                        Assert.AreEqual(null, db.GetAsString("k" + i));
                    }

                    for (var i = 5; i < 10; i++)
                    {
                        Assert.AreEqual("v" + i, db.GetAsString("k" + i));
                    }

                    Assert.IsTrue(db.Seek("k8")
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(Enumerable.Range(8, 2).Select(i => $"k{i} - v{i}"))
                    );

                    Assert.AreEqual(null, db.Get("not exists"));
                    Assert.AreEqual(null, db.Get("k0"));

                    Assert.IsTrue(db.Seek("a").Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(db.Seek("b").Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}"))
                    );

                    // not found
                    Assert.IsTrue(db.Seek("xxx").Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(db.Seek("yyy").Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}"))
                    );
                }
            }
            finally
            {
                Directory.Delete(dbpath, true);
            }
        }
    }
}
