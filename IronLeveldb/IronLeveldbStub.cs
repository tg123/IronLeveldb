using System;
using System.Collections.Generic;
using System.Linq;
using IronLevelDB.DB;

namespace IronLevelDB
{
    internal class IronLeveldbStub : IIronLeveldb
    {
        private readonly ISeekable<InternalKey, InternalIByteArrayKeyValuePair> _dataProvider;
        private readonly Action _onDispose;
        private readonly IIronLeveldbOptions _options;

        public IronLeveldbStub(IIronLeveldbOptions options,
            ISeekable<InternalKey, InternalIByteArrayKeyValuePair> dataProvider, Action onDispose = null)
        {
            _options = options;
            _dataProvider = dataProvider;
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
        }

        public IEnumerable<IReadonlyBytesKeyValuePair> Seek(IReadOnlyList<byte> key)
        {
            // TODO snapshot is not support, use ulong.MaxValue (smallest) instead
            var interkey = new InternalKey(key.ToArray(), InternalKey.MaxSequenceNumber, InternalKey.ValueTypeForSeek);
            return _dataProvider.Seek(interkey).FilterDeleted(_options.Comparer);
        }

        public IEnumerable<IReadonlyBytesKeyValuePair> SeekFirst()
        {
            return _dataProvider.SeekFirst().FilterDeleted(_options.Comparer);
        }
    }
}
