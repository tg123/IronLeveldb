namespace IronLevelDB.SSTable
{
    internal class ByteArrayKeyValuePair : IByteArrayKeyValuePair
    {
        public ByteArrayKeyValuePair(byte[] key, byte[] value)
        {
            Key = key;
            Value = value;
        }

        public byte[] Key { get; }
        public byte[] Value { get; }
    }
}
