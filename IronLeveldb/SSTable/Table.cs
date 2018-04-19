using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf;
using IronLeveldb.Cache;
using IronLeveldb.Cache.LRU;
using IronLeveldb.DB;
using Snappy.Sharp;

namespace IronLeveldb.SSTable
{
    internal class Table : ISeekable<InternalKey, InternalIByteArrayKeyValuePair>, IChargeValue
    {
        // 1-byte type + 32-bit crc
        private const int BlockTrailerSize = 5;

        private readonly ICache _cache;
        private readonly long _cacheId;
        private readonly InternalKeyComparer _comparer;
        private readonly IContentReader _contentReader;
        private readonly ISnappyDecompressor _decompressor;

        private readonly Block _indexBlock;

        public Table(IContentReader contentReader, ICache cache, InternalKeyComparer comparer,
            ISnappyDecompressor decompressor)
        {
            _contentReader = contentReader;
            _comparer = comparer;
            _decompressor = decompressor;

            _cache = cache;
            _cacheId = IdGenerator.NewId();

            var size = contentReader.ContentLength;

            if (size < Footer.EncodedLength)
            {
                throw new InvalidDataException("file is too short to be an sstable");
            }

            // TODO replace with read
            var footers = contentReader.ReadContent(size - Footer.EncodedLength, Footer.EncodedLength);

            var footer = new Footer(footers);

            _indexBlock = new Block(ReadBlock(footer.IndexHandle), comparer);
        }

        public long Charge => _indexBlock.Charge;

        public IEnumerable<InternalIByteArrayKeyValuePair> Seek(InternalKey key)
        {
            return ToBlocks(_indexBlock.Seek(key)).SelectMany((b, i) => i == 0 ? b.Seek(key) : b.SeekFirst());
        }

        public IEnumerable<InternalIByteArrayKeyValuePair> SeekFirst()
        {
            return ToBlocks(_indexBlock.SeekFirst()).SelectMany(b => b.SeekFirst());
        }

        private IEnumerable<Block> ToBlocks(IEnumerable<InternalIByteArrayKeyValuePair> indexes)
        {
            foreach (var indexHandle in indexes)
            {
                var cachekey = indexHandle.Value;

                var block = _cache.Lookup<Block>(_cacheId, cachekey);

                if (block == null)
                {
                    // TODO here sometimes might cause read stream twice
                    var pb = new CodedInputStream(indexHandle.Value.ToArray());
                    var blockHandle = new BlockHandle(pb);

                    block = new Block(ReadBlock(blockHandle), _comparer);
                    _cache.Insert(_cacheId, cachekey, block);
                }

                yield return block;
            }
        }

        private byte[] ReadBlock(BlockHandle handle)
        {
            //            var br = new BinaryReader(stream);
            //            stream.Position = handle.Offset;

            // TODO optimize mem usage
            // TODO max int byte[]

            var n = (int) handle.Size;
            var data = _contentReader.ReadContent(handle.Offset, n + BlockTrailerSize);

            if (data.Length != n + BlockTrailerSize)
            {
                throw new InvalidDataException("truncated block read");
            }

            // TODO crc32 checksum


            switch ((CompressionType) data[n])
            {
                case CompressionType.NoCompression:
                    Array.Resize(ref data, n);
                    return data;
                case CompressionType.SnappyCompression:
                    return _decompressor.Decompress(data, 0, n);
                default:
                    throw new InvalidDataException("bad block type");
            }
        }

        private enum CompressionType
        {
            // NOTE: do not change the values of existing entries, as these are
            // part of the persistent format on disk.
            NoCompression = 0x0,
            SnappyCompression = 0x1
        }
    }
}
