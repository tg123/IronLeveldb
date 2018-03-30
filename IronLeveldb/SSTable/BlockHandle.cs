using Google.Protobuf;

namespace IronLevelDB.SSTable
{
    internal class BlockHandle
    {
        // TODO ulong in leveldb
        internal const long MaxEncodedLength = 10 + 10;

        internal BlockHandle(long offset, long size)
        {
            Offset = offset;
            Size = size;
        }

        internal BlockHandle(CodedInputStream pb)
            : this(pb.ReadInt64(), pb.ReadInt64())
        {
        }

        public long Offset { get; }

        public long Size { get; }
    }
}
