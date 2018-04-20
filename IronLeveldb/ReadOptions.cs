namespace IronLeveldb
{
    public class ReadOptions
    {
        internal static readonly ReadOptions Default = new ReadOptions();

        public bool FillCache { get; set; } = true;

        public bool VerifyChecksums { get; set; } = false;

//    snapshot(NULL)
    }
}
