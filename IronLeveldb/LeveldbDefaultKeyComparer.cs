using System;

namespace IronLevelDB
{
    public class LeveldbDefaultKeyComparer : IKeyComparer
    {
        public static readonly IKeyComparer Comparer = new LeveldbDefaultKeyComparer();

        public int Compare(byte[] a, byte[] b)
        {
//            const size_t min_len = (size_ < b.size_) ? size_ : b.size_;
//            int r = memcmp(data_, b.data_, min_len);
//            if (r == 0)
//            {
//                if (size_ < b.size_) r = -1;
//                else if (size_ > b.size_) r = +1;
//            }
//            return r;

            if (a == null || b == null)
            {
                throw new NullReferenceException("cannot compare null");
            }

            var minLen = Math.Min(a.Length, b.Length);
            var r = Memcmp(a, b, minLen);

            if (r == 0)
            {
                if (a.Length < b.Length)
                {
                    r = -1;
                }
                else if (a.Length > b.Length)
                {
                    r = +1;
                }
            }

            return r;
        }

        // why .net does not have this built-in
        // rewrite from http://research.microsoft.com/en-us/um/redmond/projects/invisible/src/crt/memcmp.c.htm
        private static int Memcmp(byte[] a, byte[] b, int count)
        {
            var v = 0;

            var p1 = 0;
            var p2 = 0;

            while (count-- > 0 && v == 0)
            {
                v = a[p1++] - b[p2++];
            }

            return v;
        }
    }
}