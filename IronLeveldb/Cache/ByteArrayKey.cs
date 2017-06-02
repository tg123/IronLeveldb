using System.Linq;

namespace IronLevelDB.Cache
{
    internal class ByteArrayKey
    {
        private readonly int _hashCode;
        private readonly byte[] _key;

        public ByteArrayKey(byte[] key)
        {
            // shoud copy yet private
            _key = key;
            _hashCode = HashCode(key);
        }

        protected bool Equals(ByteArrayKey other)
        {
            if (_hashCode != other._hashCode)
            {
                return false;
            }

            return _key.SequenceEqual(other._key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ByteArrayKey) obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        // copy from jdk Arrays
        private static int HashCode(byte[] a)
        {
            if (a == null)
            {
                return 0;
            }

            var result = 1;
            foreach (var element in a)
            {
                result = 31 * result + element;
            }

            return result;
        }
    }
}