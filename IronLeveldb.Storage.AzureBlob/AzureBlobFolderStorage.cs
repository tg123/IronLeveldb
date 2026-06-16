using System;
using System.IO;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using IronLeveldb.Storage;

namespace IronLeveldb.Storage.AzureBlob
{
    public class AzureBlobFolderStorage : IIronLeveldbStorge
    {
        private readonly BlobContainerClient _container;
        private readonly string _prefix;

        public AzureBlobFolderStorage(BlobContainerClient container, string folder = null)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _prefix = string.IsNullOrEmpty(folder) ? string.Empty : folder.TrimEnd('/') + "/";
        }

        private BlobClient GetBlob(string name)
        {
            return _container.GetBlobClient(_prefix + name);
        }

        public void Dispose()
        {
        }

        public IContentReader GetCurrentDescriptorContent()
        {
            var current = GetBlob(FileNameMaker.CurrentFileName())
                .DownloadContent().Value.Content.ToString().Trim();

            if (string.IsNullOrEmpty(current))
            {
                throw new InvalidDataException("bad CURRENT file");
            }

            return new StreamContentReader(GetBlob(current).OpenRead());
        }

        public IContentReader GetTableContentById(ulong num)
        {
            return new BlockBlobContentReader(GetBlob(FileNameMaker.TableFileName(num)));
        }

        private class BlockBlobContentReader : IContentReader
        {
            private readonly BlobClient _blob;

            public BlockBlobContentReader(BlobClient blob)
            {
                _blob = blob;
                ContentLength = blob.GetProperties().Value.ContentLength;
            }

            public long ContentLength { get; }

            public int ReadContentInto(long pos, byte[] buffer, int offset, int size)
            {
                var response = _blob.DownloadStreaming(new BlobDownloadOptions
                {
                    Range = new HttpRange(pos, size)
                });

                using (var stream = response.Value.Content)
                {
                    var read = 0;
                    int len;
                    while (read < size &&
                           (len = stream.Read(buffer, offset + read, size - read)) > 0)
                    {
                        read += len;
                    }

                    return read;
                }
            }

            public void Dispose()
            {
            }
        }
    }
}
