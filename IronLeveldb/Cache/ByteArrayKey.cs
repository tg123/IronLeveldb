using System.Collections.Generic;

namespace IronLevelDB.Cache
{
    internal class ByteArrayKey
    {
        private readonly int _hashCode;
        private readonly IReadOnlyList<byte> _key;
        private readonly long _namespaceId;

        public ByteArrayKey(long namespaceId, IReadOnlyList<byte> key)
        {
            _namespaceId = namespaceId;
            _key = key;
            _hashCode = HashCode(key);
        }

        protected bool Equals(ByteArrayKey other)
        {
            if (_hashCode != other._hashCode)
            {
                return false;
            }

            if (_namespaceId != other._namespaceId)
            {
                return false;
            }

            if (_key.Count != other._key.Count)
            {
                return false;
            }

            for (var i = _key.Count - 1; i >= 0; i--)
            {
                if (_key[i] != other._key[i])
                {
                    return false;
                }
            }

            return true;
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
            var h = _namespaceId * 31 + _hashCode;
            return (int) (h ^ (h >> 32));
        }

        // copy from jdk Arrays
        private static int HashCode(IReadOnlyList<byte> a)
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
