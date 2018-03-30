using System.IO;
using Google.Protobuf;

namespace IronLevelDB.SSTable
{
    internal class Footer
    {
        // kTableMagicNumber was picked by running
        //    echo http://code.google.com/p/leveldb/ | sha1sum
        // and taking the leading 64 bits.
        private const ulong TableMagicNumber = 0xdb4775248b80fb57ul;

        public const long EncodedLength = BlockHandle.MaxEncodedLength * 2 + sizeof(long);

        public Footer(Stream stream)
        {
            var pos = stream.Position;
            stream.Seek(EncodedLength - sizeof(long), SeekOrigin.Current);
            var pb = new CodedInputStream(stream);

            // TODO BitConverter.IsLittleEndian == true
            var magic = pb.ReadFixed64();

            if (magic != TableMagicNumber)
            {
                throw new InvalidDataException("not an sstable (bad magic number)");
            }

            stream.Position = pos;

            MetaIndexHandle = new BlockHandle(pb);
            IndexHandle = new BlockHandle(pb);
        }

        public BlockHandle MetaIndexHandle { get; }
        public BlockHandle IndexHandle { get; }
    }
}
