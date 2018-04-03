using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf;
using IronLevelDB.Cache;
using IronLevelDB.Cache.LRU;
using IronLevelDB.DB;

namespace IronLevelDB.SSTable
{
    internal class Table : ISeekable<InternalKey, InternalIByteArrayKeyValuePair>, IChargeValue
    {
        private readonly ICache _cache;
        private readonly long _cacheId;
        private readonly InternalKeyComparer _comparer;
        private readonly IContentReader _contentReader;

        private readonly Block _indexBlock;

        public Table(IContentReader contentReader, ICache cache, InternalKeyComparer comparer)
        {
            _contentReader = contentReader;
            _comparer = comparer;

            _cache = cache;
            _cacheId = IdGenerator.NewId();

            var size = contentReader.ContentLength;

            if (size < Footer.EncodedLength)
            {
                throw new InvalidDataException("file is too short to be an sstable");
            }

            // TODO replace with read
            var footers =
                new MemoryStream(contentReader.ReadContent(size - Footer.EncodedLength, Footer.EncodedLength));

            //  stream.Seek(-Footer.EncodedLength, SeekOrigin.End);
            var footer = new Footer(footers);

            _indexBlock = new Block(_contentReader.ReadBlock(footer.IndexHandle), comparer);
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

                    block = new Block(_contentReader.ReadBlock(blockHandle), _comparer);
                    _cache.Insert(_cacheId, cachekey, block);
                }

                yield return block;
            }
        }
    }
}
