namespace IronLevelDB.Cache
{
    public class NoCache : ICache
    {
        public void Prune()
        {
        }

        public long NewId()
        {
            return IdGenerator.NewId();
        }

        public void Insert<T>(byte[] key, T value)
        {
        }

        public T Lookup<T>(byte[] key)
        {
            return default(T);
        }

        public void Erase(byte[] key)
        {
        }

        public void Insert<T>(string key, T value)
        {
        }
    }
}
