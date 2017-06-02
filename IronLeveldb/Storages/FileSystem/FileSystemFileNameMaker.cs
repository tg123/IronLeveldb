namespace IronLevelDB.Storages.FileSystem
{
    internal class FileSystemFileNameMaker
    {
        public FileSystemFileNameMaker(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public string CurrentFileName()
        {
            return FullPath(FileNameMaker.CurrentFileName());
        }

        public string DescriptorFileName(ulong number)
        {
            return FullPath(FileNameMaker.DescriptorFileName(number));
        }

        public string FullPath(string filename)
        {
            return System.IO.Path.Combine(Path, filename);
        }

        public string TableFileName(ulong number)
        {
            return FullPath(FileNameMaker.TableFileName(number));
        }
    }
}