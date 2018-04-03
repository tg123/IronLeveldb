namespace IronLevelDB.DB
{
    internal struct FileMetaData
    {
        public ulong Number { get; set; }
        public ulong FileSize { get; set; } // File size in bytes
        public InternalKey Smallest { get; set; } // Smallest internal key served by table
        public InternalKey Largest { get; set; } // Largest internal key served by tabl
    }
}
