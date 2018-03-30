using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Google.Protobuf;
using IronLevelDB.Cache.LRU;
using IronLevelDB.DB;

namespace IronLevelDB.SSTable
{
    internal class Block : ISeekable<InternalKey, InternalIByteArrayKeyValuePair>, IChargeValue
    {
        // TODO option?
        private const int DefaultRestartInterval = 16;

        private readonly InternalKeyComparer _comparer;

        private readonly byte[] _data;

        private readonly int _numRestarts;
        private readonly int _restartOffset;

//        public Block(Stream stream, BlockHandle handle, IKeyComparer comparer)
//            : this(ReadBlock(stream, handle), comparer)
//        {
//        }

        internal Block(byte[] data, InternalKeyComparer comparer)
        {
            _data = data;
            _comparer = comparer;
            if (_data.Length < sizeof(uint))
            {
                throw new InvalidDataException("bad block contents - block length");
            }

            _numRestarts = BitConverter.ToInt32(_data, _data.Length - sizeof(uint));

            var maxRestartsAllowed = (_data.Length - sizeof(uint)) / sizeof(uint);
            if (_numRestarts > maxRestartsAllowed)
            {
                throw new InvalidDataException("bad block contents - numRestarts");
            }
            _restartOffset = _data.Length - (1 + _numRestarts) * sizeof(uint);
        }

        public long Charge => _data.Length;

        public IEnumerable<InternalIByteArrayKeyValuePair> Seek(InternalKey target)
        {
            var left = 0;
            var right = _numRestarts - 1;

            var value = default(ArraySegment<byte>); // not used
            var keybuf = new AppendableByteArraySegment(DefaultRestartInterval);

            while (left < right)
            {
                var mid = (left + right + 1) / 2;

                keybuf.TrimToLength(0);
                ParseEntry(GetRestartPointOffset(mid), ref keybuf, ref value);

                var midKey = new InternalKey(new ArraySegment<byte>(keybuf.ToArray()));

                if (_comparer.Compare(midKey, target) < 0)
                {
                    // Key at "mid" is smaller than "target".  Therefore all
                    // blocks before "mid" are uninteresting.
                    left = mid;
                }
                else
                {
                    // Key at "mid" is >= "target".  Therefore all blocks at or
                    // after "mid" are uninteresting.
                    right = mid - 1;
                }
            }

            // Linear search (within restart block) for first key >= target
            return SeekToRestartPoint(left).SkipWhile(kv => _comparer.Compare(kv.InternalKey, target) < 0);
        }

        public IEnumerable<InternalIByteArrayKeyValuePair> SeekFirst()
        {
            return SeekToRestartPoint(0);
        }

        private IEnumerable<InternalIByteArrayKeyValuePair> SeekToOffset(int offset, AppendableByteArraySegment lastkey)
        {
            var key = lastkey;
            var value = new ArraySegment<byte>(_data, offset, 0);

            for (;;)
            {
                var currentOffset = value.Offset + value.Count;

                if (currentOffset >= _restartOffset)
                {
                    yield break;
                }

                ParseEntry(currentOffset, ref key, ref value);

                yield return new InternalIByteArrayKeyValuePair(new ByteArrayKeyValuePair(key.ToArray(), value.ToArray()));
//                yield return new ArraySegByteArrayKeyValuePair(
//                    key.Clone(),
//                    new ArraySegment<byte>(value.Array, value.Offset, value.Count));
            }
        }

        private IEnumerable<InternalIByteArrayKeyValuePair> SeekToRestartPoint(int restartPointIndex)
        {
            return SeekToOffset(GetRestartPointOffset(restartPointIndex),
                new AppendableByteArraySegment(DefaultRestartInterval));
        }


        // ref key current not use
        private void ParseEntry(int offset, ref AppendableByteArraySegment key, ref ArraySegment<byte> value)
        {
            var pb = new CodedInputStream(_data, offset, _restartOffset - offset);

            var shared = pb.ReadInt32();
            var nonShared = pb.ReadInt32();
            var valueLength = pb.ReadInt32();

            if (key.Length < shared)
            {
                throw new InvalidDataException("bad entry in block - shared > _key.length");
            }

            var p = (int) pb.Position;

            key.TrimToLength(shared);
            key.Append(new ArraySegment<byte>(_data, p, nonShared));
            value = new ArraySegment<byte>(_data, p + nonShared, valueLength);
        }

        private /*unsafe*/ int GetRestartPointOffset(int index)
        {
            Debug.Assert(index < _numRestarts);
            return BitConverter.ToInt32(_data, _restartOffset + index * sizeof(uint));

//            fixed (byte* numPtr = &_data[_restartOffset + index * sizeof(uint)])
//            {
//                return *(int*) numPtr;
//            }
        }
    }
}