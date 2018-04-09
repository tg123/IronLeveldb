using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace IronLeveldb.Storage.AzureBlob
{
    public class AzureBlobFolderStorage : IIronLeveldbStorge
    {
        private readonly CloudBlobDirectory[] _folders;

        public AzureBlobFolderStorage(CloudBlobDirectory folder, IEnumerable<CloudBlobDirectory> mirrors = null)
        {
            _folders = new[] {folder}.Concat(mirrors ?? Enumerable.Empty<CloudBlobDirectory>()).ToArray();
        }

        public void Dispose()
        {
        }

        public Stream GetCurrentDescriptorContent()
        {
            var current = _folders[0].GetBlockBlobReference(FileNameMaker.CurrentFileName()).DownloadTextAsync().Result
                .Trim();

            if (string.IsNullOrEmpty(current))
            {
                throw new InvalidDataException("bad CURRENT file");
            }

            return _folders[0].GetBlockBlobReference(current).OpenReadAsync().Result;
        }

        public Stream GetDescriptorContentById(ulong num)
        {
            return _folders[0].GetBlockBlobReference(FileNameMaker.DescriptorFileName(num)).OpenReadAsync().Result;
        }

        public IContentReader GetTableContentById(ulong num)
        {
            return new BlockBlobContentReader(
                _folders.Select(f => f.GetBlockBlobReference(FileNameMaker.TableFileName(num))).ToArray());
        }

        private class BlockBlobContentReader : IContentReader
        {
            private readonly CloudBlockBlob[] _blobs;

            private uint _rrid;

            public BlockBlobContentReader(CloudBlockBlob[] blobs)
            {
                _blobs = blobs;
                blobs[0].FetchAttributesAsync().Wait();
                ContentLength = blobs[0].Properties.Length;
            }

            public long ContentLength { get; }

            public byte[] ReadContent(long offset, long size)
            {
                var buff = new byte[size];
                // TODO add policy
                _blobs[_rrid++ % _blobs.Length].DownloadRangeToByteArrayAsync(buff, 0, offset, size)
                    .Wait(); // TODO dont block
                return buff;
            }
        }
    }
}
