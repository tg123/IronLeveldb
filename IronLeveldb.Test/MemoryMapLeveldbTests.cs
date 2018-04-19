using IronLeveldb.Storage.Mmap;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronLeveldb.Test
{
    [TestClass]
    public class MemoryMapLeveldbTests : ReadonlyFileSystemLeveldbTests
    {
        protected override IIronLeveldb CreateIronLeveldb(string dbpath)
        {
            return new ReadonlyMemoryMappedStorage(dbpath).Build();
        }
    }
}
