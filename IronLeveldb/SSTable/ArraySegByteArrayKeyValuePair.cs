using System;
using System.Linq;

namespace IronLevelDB.SSTable
{
    internal class ArraySegByteArrayKeyValuePair : IByteArrayKeyValuePair
    {
//        private readonly AppendableByteArraySegment _key;
//        private readonly Lazy<byte[]> _keyLazy;
//        private readonly ArraySegment<byte> _value;
//        private readonly Lazy<byte[]> _valueLazy;


        public ArraySegByteArrayKeyValuePair(AppendableByteArraySegment key, ArraySegment<byte> value)
        {
//            _key = key;
//            _value = value;
//            _keyLazy = new Lazy<byte[]>(key.ToArray);
//            _valueLazy = new Lazy<byte[]>(() => value.ToArray());
            Key = key.ToArray();
            Value = value.ToArray();
        }

        public byte[] Key { get; }

        public byte[] Value { get; }
//        ArraySegment<byte> IKeyValuePair<ArraySegment<byte>, ArraySegment<byte>>.Value => _value;
//

//        ArraySegment<byte> IKeyValuePair<ArraySegment<byte>, ArraySegment<byte>>.Key => _key;

//        byte[] IKeyValuePair<byte[], byte[]>.Key => _keyLazy.Value;
//        byte[] IKeyValuePair<byte[], byte[]>.Value => _valueLazy.Value;
    }
}
