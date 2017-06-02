using System;
using System.Collections.Generic;
using IronLevelDB.DB;

namespace IronLevelDB
{
    internal class IronLeveldbStub : IIronLeveldb
    {
        private readonly ISeekable<byte[], InternalIByteArrayKeyValuePair> _dataProvider;
        private readonly Action _onDispose;
        private readonly IIronLeveldbOptions _options;

        public IronLeveldbStub(IIronLeveldbOptions options,
            ISeekable<byte[], InternalIByteArrayKeyValuePair> dataProvider, Action onDispose = null)
        {
            _options = options;
            _dataProvider = dataProvider;
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
        }

        public IEnumerable<IByteArrayKeyValuePair> Seek(byte[] key)
        {
            return _dataProvider.Seek(key).FilterDeleted(_options.Comparer);
        }

        public IEnumerable<IByteArrayKeyValuePair> SeekFirst()
        {
            return _dataProvider.SeekFirst().FilterDeleted(_options.Comparer);
        }
    }
}