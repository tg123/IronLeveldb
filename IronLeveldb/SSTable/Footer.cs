using System;
using System.IO;
using Google.Protobuf;

namespace IronLeveldb.SSTable
{
    internal class Footer
    {
        // kTableMagicNumber was picked by running
        //    echo http://code.google.com/p/leveldb/ | sha1sum
        // and taking the leading 64 bits.
        private const ulong TableMagicNumber = 0xdb4775248b80fb57ul;

        public const int EncodedLength = BlockHandle.MaxEncodedLength * 2 + sizeof(long);

        public Footer(byte[] footerContent)
        {
            var magic = BitConverter.ToUInt64(footerContent, EncodedLength - sizeof(long));

            if (magic != TableMagicNumber)
            {
                throw new InvalidDataException("not an sstable (bad magic number)");
            }

            var pb = new CodedInputStream(footerContent);
            MetaIndexHandle = new BlockHandle(pb);
            IndexHandle = new BlockHandle(pb);
        }

        public BlockHandle MetaIndexHandle { get; }
        public BlockHandle IndexHandle { get; }
    }
}
