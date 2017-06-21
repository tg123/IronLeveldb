﻿using System;
using System.Text;

namespace IronLevelDB.SSTable
{
    internal class AppendableByteArraySegment
    {
        private readonly ArraySegment<byte>[] _arraySegments;
        private int _p;

        public AppendableByteArraySegment(int maxSegmentCount)
        {
            _arraySegments = new ArraySegment<byte>[maxSegmentCount];
        }

        public int Length { get; private set; }

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

    internal static class AppendableByteArraySegmentExt
    {
        public static void Append(this AppendableByteArraySegment abs, byte[] array)
        {
            abs.Append(new ArraySegment<byte>(array));
        }

        public static void Append(this AppendableByteArraySegment abs, string str)
        {
            abs.Append(str, Encoding.UTF8);
        }

        public static void Append(this AppendableByteArraySegment abs, string str, Encoding encoding)
        {
            abs.Append(encoding.GetBytes(str));
        }

        public static string GetString(this AppendableByteArraySegment abs, Encoding encoding)
        {
            return encoding.GetString(abs.ToArray());
        }

        public static string GetString(this AppendableByteArraySegment abs)
        {
            return abs.GetString(Encoding.UTF8);
        }
    }
}