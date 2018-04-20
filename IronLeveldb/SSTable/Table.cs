using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Force.Crc32;
using Google.Protobuf;
using IronLeveldb.Cache;
using IronLeveldb.Cache.LRU;
using IronLeveldb.DB;

namespace IronLeveldb.SSTable
{
    internal class Table : ISeekable<InternalKey, InternalIByteArrayKeyValuePair>, IChargeValue
    {
        // 1-byte type + 32-bit crc
        private const int BlockTrailerSize = 5;

        private readonly ICache _cache;
        private readonly long _cacheId;
        private readonly IContentReader _contentReader;

        private readonly Block _indexBlock;
        private readonly IronLeveldbOptions _options;

        public Table(IContentReader contentReader, IronLeveldbOptions options)
        {
            _contentReader = contentReader;
            _options = options;

            _cache = options.BlockCache;
            _cacheId = IdGenerator.NewId();

            var size = contentReader.ContentLength;

            if (size < Footer.EncodedLength)
            {
                throw new InvalidDataException("file is too short to be an sstable");
            }

            // TODO replace with read
            var footers = contentReader.ReadContent(size - Footer.EncodedLength, Footer.EncodedLength);

            var footer = new Footer(footers);

            _indexBlock = new Block(ReadBlock(footer.IndexHandle, options.ParanoidChecks), options.InternalKeyComparer);
        }

        public long Charge => _indexBlock.Charge;

        public IEnumerable<InternalIByteArrayKeyValuePair> Seek(InternalKey key, ReadOptions options)
        {
            return ToBlocks(_indexBlock.Seek(key, options), options)
                .SelectMany((b, i) => i == 0 ? b.Seek(key, options) : b.SeekFirst(options));
        }

        public IEnumerable<InternalIByteArrayKeyValuePair> SeekFirst(ReadOptions options)
        {
            return ToBlocks(_indexBlock.SeekFirst(options), options).SelectMany(b => b.SeekFirst(options));
        }

        private IEnumerable<Block> ToBlocks(IEnumerable<InternalIByteArrayKeyValuePair> indexes, ReadOptions options)
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

                    block = new Block(ReadBlock(blockHandle, options.VerifyChecksums), _options.InternalKeyComparer);

                    if (options.FillCache)
                    {
                        _cache.Insert(_cacheId, cachekey, block);
                    }
                }

                yield return block;
            }
        }

        private byte[] ReadBlock(BlockHandle handle, bool verifyChecksums)
        {
            // TODO max int byte[]
            var n = (int) handle.Size;
            var data = _contentReader.ReadContent(handle.Offset, n + BlockTrailerSize);

            if (data.Length != n + BlockTrailerSize)
            {
                throw new InvalidDataException("truncated block read");
            }

            if (verifyChecksums)
            {
                /*
                static const uint32_t kMaskDelta = 0xa282ead8ul;

                // Return a masked representation of crc.
                //
                // Motivation: it is problematic to compute the CRC of a string that
                // contains embedded CRCs.  Therefore we recommend that CRCs stored
                // somewhere (e.g., in files) should be masked before being stored.
                inline uint32_t Mask(uint32_t crc) {
                    // Rotate right by 15 bits and add a constant.
                    return ((crc >> 15) | (crc << 17)) + kMaskDelta;
                }

                // Return the crc whose masked representation is masked_crc.
                inline uint32_t Unmask(uint32_t masked_crc) {
                    uint32_t rot = masked_crc - kMaskDelta;
                    return ((rot >> 17) | (rot << 15));
                }
                */

                uint Unmask(uint maskedCrc)
                {
                    const uint kMaskDelta = 0xa282ead8;

                    var rot = maskedCrc - kMaskDelta;
                    return (rot >> 17) | (rot << 15);
                }

                var crc = Unmask(BitConverter.ToUInt32(data, n + 1));
                var actual = Crc32CAlgorithm.Compute(data, 0, n + 1);

                if (crc != actual)
                {
                    throw new InvalidDataException("block checksum mismatch");
                }
            }

            switch ((CompressionType) data[n])
            {
                case CompressionType.NoCompression:
                    Array.Resize(ref data, n);
                    return data;
                case CompressionType.SnappyCompression:
                    return _options.SnappyDecompressor.Decompress(data, 0, n);
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
