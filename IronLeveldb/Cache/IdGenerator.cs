using System.Threading;

namespace IronLevelDB.Cache
{
    public static class IdGenerator
    {
        private static long _id;

        public static long NewId()
        {
            return Interlocked.Increment(ref _id);
        }
    }
}
