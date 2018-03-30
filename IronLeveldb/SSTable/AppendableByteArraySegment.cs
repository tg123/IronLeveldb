using System;
using System.Collections;
using System.Collections.Generic;

namespace IronLevelDB.SSTable
{
    internal class AppendableByteArraySegment : IReadOnlyCollection<byte>
    {
        private readonly ArraySegment<byte>[] _arraySegments;
        private int _p;

        public AppendableByteArraySegment(int maxSegmentCount)
        {
            _arraySegments = new ArraySegment<byte>[maxSegmentCount];
        }

        public int Length { get; private set; }

        public IEnumerator<byte> GetEnumerator()
        {
            for (var i = 0; i < _p; i++)
            {
                var arraySegment = _arraySegments[i];
                foreach (var b in arraySegment)
                {
                    yield return b;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => Length;

        public void Append(ArraySegment<byte> array)
        {
            if (_p >= _arraySegments.Length)
            {
                throw new OverflowException();
            }

            _arraySegments[_p] = array;
            _p += 1;
            Length += array.Count;
        }

        public void TrimToLength(int length)
        {
            if (Length < length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            while (Length > length)
            {
                var c = _arraySegments[_p - 1];
                if (Length - c.Count >= length)
                {
                    Length -= c.Count;
                    _p -= 1;
                }
                else
                {
                    var delta = Length - length;
                    _arraySegments[_p - 1] = new ArraySegment<byte>(c.Array, c.Offset, c.Count - delta);
                    Length -= delta;
                }
            }
        }

        public byte[] ToArray()
        {
            var merged = new byte[Length];

            var offset = 0;

            for (var i = 0; i < _p; i++)
            {
                var arraySegment = _arraySegments[i];
                Buffer.BlockCopy(arraySegment.Array, arraySegment.Offset, merged, offset, arraySegment.Count);
                offset += arraySegment.Count;
            }

            // TODO test only
            _arraySegments[0] = new ArraySegment<byte>(merged);
            _p = 1;

            return merged;
        }

        public AppendableByteArraySegment Clone()
        {
            var s = new AppendableByteArraySegment(_arraySegments.Length);

            for (var i = 0; i < _p; i++)
            {
                var arraySegment = _arraySegments[i];
                s.Append(arraySegment);
            }

            return s;
        }
    }
}
