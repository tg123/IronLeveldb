using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IronLevelDB.DB
{
    internal class MergingEnumerable<T> : IEnumerable<T>
    {
        private readonly IComparer<T> _comparer;

        private readonly IEnumerable<IEnumerable<T>> _iters;

        public MergingEnumerable(IEnumerable<IEnumerable<T>> iters) : this(iters, Comparer<T>.Default)
        {
        }

        public MergingEnumerable(IEnumerable<IEnumerable<T>> iters, IComparer<T> comparer)
        {
            _iters = iters;
            _comparer = comparer;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            var iters = _iters.Select((t, p) => new InternalValue(p, t, _comparer).Next()).Where(t => t != null);

            var s = new SortedSet<InternalValue>(iters);

            while (s.Count > 0)
            {
                var c = s.Min;

                s.Remove(c);

                yield return c.Current;

                c = c.Next();

                if (c != null)
                {
                    s.Add(c);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class InternalValue : IComparable<InternalValue>
        {
            private readonly IComparer<T> _comparer;
            private readonly IEnumerator<T> _enumerator;
            private readonly int _priority;

            public T Current;

            public InternalValue(int priority, IEnumerable<T> values, IComparer<T> comparer)
            {
                _priority = priority;
                _comparer = comparer;
                _enumerator = values.GetEnumerator();
            }

            public int CompareTo(InternalValue other)
            {
                var c = _comparer.Compare(Current, other.Current);

                if (c == 0)
                {
                    return _priority - other._priority;
                }

                return c;
            }

            internal InternalValue Next()
            {
                if (_enumerator.MoveNext())
                {
                    Current = _enumerator.Current;
                    return this;
                }

                return null;
            }
        }
    }
}
