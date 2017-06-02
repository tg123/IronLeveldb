using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf;
using IronLevelDB.Cache.LRU;

namespace IronLevelDB.SSTable
{
    internal class Table : ISeekable<byte[], IByteArrayKeyValuePair>, IChargeValue
    {
        private readonly ICache _cache;
        private readonly long _cacheId;
        private readonly IKeyComparer _comparer;
        private readonly IContentReader _contentReader;

        private readonly Block _indexBlock;
//        private readonly Stream _stream;
//        private readonly object _streamlock = new object();

        public Table(IContentReader contentReader, ICache cache, IKeyComparer comparer)
        {
//            _stream = stream;
            _contentReader = contentReader;
            _comparer = comparer;

            _cache = cache;
            _cacheId = _cache.NewId();

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

            //            _indexBlock = new Lazy<Block>(() =>
            //            {
            //                stream.Seek(-Footer.EncodedLength, SeekOrigin.End);
            //                var footer = new Footer(stream);
            //
            //                return new Block(stream, footer.IndexHandle, comparer);
            //            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public long Charge => _indexBlock.Charge;

        public IEnumerable<IByteArrayKeyValuePair> Seek(byte[] key)
        {
            return ToBlocks(_indexBlock.Seek(key)).SelectMany((b, i) => i == 0 ? b.Seek(key) : b.SeekFirst());
        }

        public IEnumerable<IByteArrayKeyValuePair> SeekFirst()
        {
            return ToBlocks(_indexBlock.SeekFirst()).SelectMany(b => b.SeekFirst());
        }

        private IEnumerable<Block> ToBlocks(IEnumerable<IByteArrayKeyValuePair> indexes)
        {
            foreach (var indexHandle in indexes)
            {
                var cachekey = indexHandle.Value;

                var block = _cache.Lookup<Block>(_cacheId, cachekey);

                if (block == null)
                {
                    // TODO here sometimes might cause read stream twice
                    var pb = new CodedInputStream(indexHandle.Value);
                    var blockHandle = new BlockHandle(pb);

//                    lock (_streamlock)
//                    {
                    block = new Block(_contentReader.ReadBlock(blockHandle), _comparer);
//                    }
                    _cache.Insert(_cacheId, cachekey, block);
                }

                yield return block;
            }
        }
    }
}