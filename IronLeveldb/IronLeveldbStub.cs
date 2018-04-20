using System;
using System.Collections.Generic;
using System.Linq;
using IronLeveldb.DB;

namespace IronLeveldb
{
    internal class IronLeveldbStub : IIronLeveldb
    {
        private readonly IKeyComparer _comparer;
        private readonly ISeekable<InternalKey, InternalIByteArrayKeyValuePair> _dataProvider;
        private readonly Action _onDispose;

        public IronLeveldbStub(IronLeveldbOptions options,
            ISeekable<InternalKey, InternalIByteArrayKeyValuePair> dataProvider, Action onDispose = null)
        {
            _dataProvider = dataProvider;
            _onDispose = onDispose;
            _comparer = options?.Comparer ?? LeveldbDefaultKeyComparer.Comparer;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
        }

        public IEnumerable<IReadonlyBytesKeyValuePair> Seek(IReadOnlyList<byte> key, ReadOptions options)
        {
            // TODO snapshot is not support, use ulong.MaxValue (smallest) instead
            var interkey = new InternalKey(key.ToArray(), InternalKey.MaxSequenceNumber, InternalKey.ValueTypeForSeek);
            return FilterDeleted(_dataProvider.Seek(interkey, options));
        }

        public IEnumerable<IReadonlyBytesKeyValuePair> SeekFirst(ReadOptions options)
        {
            return FilterDeleted(_dataProvider.SeekFirst(options));
        }

        private IEnumerable<InternalIByteArrayKeyValuePair> FilterDeleted(
            IEnumerable<InternalIByteArrayKeyValuePair> internalkvs)
        {
            IReadOnlyList<byte> lastkey = null;

            foreach (var kv in internalkvs)
            {
                switch (kv.InternalKey.Type)
                {
                    case InternalKey.ValueType.Deletion:
                        lastkey = kv.Key;

                        break;
                    case InternalKey.ValueType.Value:
                        if (lastkey != null && _comparer.Compare(kv.Key, lastkey) <= 0)
                        {
                            // skip
                        }
                        else
                        {
                            yield return kv;
                            lastkey = kv.Key;
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
