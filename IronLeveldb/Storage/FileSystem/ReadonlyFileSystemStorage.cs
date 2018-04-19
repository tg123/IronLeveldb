using System.IO;
using System.Linq;

namespace IronLeveldb.Storage.FileSystem
{
    public class ReadonlyFileSystemStorage : ReadonlyStreamStorage
    {
        private readonly FileSystemFileNameMaker _fileSystemFileNameMaker;

        public ReadonlyFileSystemStorage(string path)
        {
            _fileSystemFileNameMaker = new FileSystemFileNameMaker(path);
        }

        protected override Stream GetCurrentDescriptorContentStream()
        {
            var current = File.ReadLines(_fileSystemFileNameMaker.CurrentFileName()).FirstOrDefault()?.Trim();

            // TODO why check new line?
            if (string.IsNullOrEmpty(current))
            {
                throw new InvalidDataException("bad CURRENT file");
            }

            return File.OpenRead(_fileSystemFileNameMaker.FullPath(current));
        }

        protected override Stream GetTableContentStreamById(ulong num)
        {
            return File.OpenRead(_fileSystemFileNameMaker.TableFileName(num));
        }
    }
}
