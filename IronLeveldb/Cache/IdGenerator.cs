using System.Threading;

namespace IronLevelDB.Cache
{
    internal static class IdGenerator
    {
        private static long _id;

        public static long NewId()
        {
            return Interlocked.Increment(ref _id);
        }
    }
}