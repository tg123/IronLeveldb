using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace IronLeveldb.Storage.AzureBlob
{
    public class AzureBlobFolderStorage : IIronLeveldbStorge
    {
        private readonly CloudBlobDirectory _folder;

        public AzureBlobFolderStorage(CloudBlobDirectory folder)
        {
            _folder = folder;
        }

        public void Dispose()
        {
        }

        public IContentReader GetCurrentDescriptorContent()
        {
            var current = _folder.GetBlockBlobReference(FileNameMaker.CurrentFileName()).DownloadTextAsync().Result
                .Trim();

            if (string.IsNullOrEmpty(current))
            {
                throw new InvalidDataException("bad CURRENT file");
            }

            return new StreamContentReader(_folder.GetBlockBlobReference(current).OpenReadAsync().Result);
        }

        public IContentReader GetTableContentById(ulong num)
        {
            return new BlockBlobContentReader(_folder.GetBlockBlobReference(FileNameMaker.TableFileName(num)));
        }

        private class BlockBlobContentReader : IContentReader
        {
            private readonly CloudBlockBlob _blob;

            public BlockBlobContentReader(CloudBlockBlob blob)
            {
                _blob = blob;
                blob.FetchAttributesAsync().Wait();
                ContentLength = blob.Properties.Length;
            }

            public long ContentLength { get; }

            public int ReadContentInto(long pos, byte[] buffer, int offset, int size)
            {
                return _blob.DownloadRangeToByteArrayAsync(buffer, offset, pos, size).Result;
            }

            public void Dispose()
            {
            }
        }
    }
}
