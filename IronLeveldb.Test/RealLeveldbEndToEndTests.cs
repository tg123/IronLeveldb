using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LevelDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronLeveldb.Test
{
    /// <summary>
    /// End-to-end tests that build a real on-disk leveldb database with the native
    /// leveldb library and then read it back with IronLeveldb. A small write buffer is
    /// used to force the data through several on-disk SST (.ldb) files, exercising the
    /// table reader, block reader and snappy block decompression paths.
    /// </summary>
    [TestClass]
    public class RealLeveldbEndToEndTests
    {
        private const int Count = 2000;

        private static string Key(int i)
        {
            return $"key{i:D6}";
        }

        private static string Value(int i)
        {
            // long enough to push the write buffer over its limit quickly and to make
            // snappy block compression worthwhile.
            return $"value{i:D6}-" + new string((char) ('a' + i % 26), 64);
        }

        private static string Pair(int i)
        {
            return $"{Key(i)} - {Value(i)}";
        }

        protected virtual IIronLeveldb CreateIronLeveldb(string dbpath)
        {
            return IronLeveldbBuilder.BuildFromPath(dbpath);
        }

        [TestMethod]
        public void ReadLargeDatasetAcrossMultipleTables()
        {
            var dbpath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dbpath);

            try
            {
                using (var db = LevelDB.DB.Open(dbpath, new Options
                {
                    CreateIfMissing = true,
                    WriteBufferSize = 4 * 1024 // tiny buffer -> many flushed SST files
                }))
                {
                    for (var i = 0; i < Count; i++)
                    {
                        db.Put(new WriteOptions(), Key(i), Value(i));
                    }
                }

                // reopen so leveldb recovers the log and flushes everything to SST files
                using (LevelDB.DB.Open(dbpath, new Options()))
                {
                }

                // more than one SST file should have been produced
                Assert.IsTrue(Directory.GetFiles(dbpath, "*.ldb").Length > 1,
                    "expected the dataset to span multiple SST files");

                using (var db = CreateIronLeveldb(dbpath))
                {
                    // point lookups across the whole range
                    for (var i = 0; i < Count; i++)
                    {
                        Assert.AreEqual(Value(i), db.GetAsString(Key(i)));
                    }

                    Assert.IsNull(db.Get("not exists"));

                    // full forward scan must be ordered and complete
                    Assert.IsTrue(db.SeekFirst()
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(Enumerable.Range(0, Count).Select(Pair)));

                    // seek to an arbitrary key returns the suffix of the ordered range
                    Assert.IsTrue(db.Seek(Key(1500))
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(Enumerable.Range(1500, Count - 1500).Select(Pair)));
                }
            }
            finally
            {
                Directory.Delete(dbpath, true);
            }
        }

        [TestMethod]
        public void ReadAfterOverwriteAndDelete()
        {
            var dbpath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dbpath);

            try
            {
                using (var db = LevelDB.DB.Open(dbpath, new Options
                {
                    CreateIfMissing = true,
                    WriteBufferSize = 4 * 1024
                }))
                {
                    for (var i = 0; i < Count; i++)
                    {
                        db.Put(new WriteOptions(), Key(i), Value(i));
                    }
                }

                // overwrite even keys, delete odd keys, in a second generation of tables
                using (var db = LevelDB.DB.Open(dbpath, new Options
                {
                    WriteBufferSize = 4 * 1024
                }))
                {
                    for (var i = 0; i < Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            db.Put(new WriteOptions(), Key(i), "updated-" + Value(i));
                        }
                        else
                        {
                            db.Delete(new WriteOptions(), Key(i));
                        }
                    }
                }

                using (LevelDB.DB.Open(dbpath, new Options()))
                {
                }

                using (var db = CreateIronLeveldb(dbpath))
                {
                    for (var i = 0; i < Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            Assert.AreEqual("updated-" + Value(i), db.GetAsString(Key(i)));
                        }
                        else
                        {
                            Assert.IsNull(db.GetAsString(Key(i)));
                        }
                    }

                    var expected = new List<string>();
                    for (var i = 0; i < Count; i += 2)
                    {
                        expected.Add($"{Key(i)} - updated-{Value(i)}");
                    }

                    Assert.IsTrue(db.SeekFirst()
                        .Select(kv => $"{kv.KeyAsString()} - {kv.ValueAsString()}")
                        .SequenceEqual(expected));
                }
            }
            finally
            {
                Directory.Delete(dbpath, true);
            }
        }
    }

    [TestClass]
    public class MemoryMapRealLeveldbEndToEndTests : RealLeveldbEndToEndTests
    {
        protected override IIronLeveldb CreateIronLeveldb(string dbpath)
        {
            return new global::IronLeveldb.Storage.Mmap.ReadonlyMemoryMappedStorage(dbpath).Build();
        }
    }
}
