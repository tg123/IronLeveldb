namespace IronLevelDB.Storages
{
    public static class FileNameMaker
    {
        public static string CurrentFileName()
        {
            return $"CURRENT";
        }

        public static string DescriptorFileName(ulong number)
        {
            return $"MANIFEST-{number:D6}";
        }

        private static string MakeFileName(ulong number, string suffix)
        {
            return $"{number:D6}.{suffix}";
        }

        public static string TableFileName(ulong number)
        {
            return MakeFileName(number, "ldb");
        }
    }
}
