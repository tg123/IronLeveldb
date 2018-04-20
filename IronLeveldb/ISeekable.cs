using System.Collections.Generic;

namespace IronLeveldb
{
    public interface ISeekable<in TK, out TV>
    {
        IEnumerable<TV> Seek(TK key, ReadOptions options);

        IEnumerable<TV> SeekFirst(ReadOptions options);

        // TODO support seeklast and reverse direction
    }

    public static class SeekableExt
    {
        public static IEnumerable<TV> Seek<TK, TV>(this ISeekable<TK, TV> seekable, TK key)
        {
            return seekable.Seek(key, ReadOptions.Default);
        }

        public static IEnumerable<TV> SeekFirst<TK, TV>(this ISeekable<TK, TV> seekable)
        {
            return seekable.SeekFirst(ReadOptions.Default);
        }
    }
}
