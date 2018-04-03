using System.Collections.Generic;

namespace IronLevelDB.DB
{
    internal class InternalKeyComparer : IComparer<InternalKey>
    {
        private readonly IKeyComparer _userComparer;

        public InternalKeyComparer(IKeyComparer userComparer)
        {
            _userComparer = userComparer;
        }

        public int Compare(InternalKey a, InternalKey b)
        {
            // Order by:
            //    increasing user key (according to user-supplied comparator)
            //    decreasing sequence number
            //    decreasing type (though sequence# should be enough to disambiguate)

            var r = _userComparer.Compare(a.UserKey, b.UserKey);

            if (r == 0)
            {
                r = -Comparer<ulong>.Default.Compare(a.Sequence, b.Sequence);
            }

            if (r == 0)
            {
                r = -Comparer<InternalKey.ValueType>.Default.Compare(a.Type, b.Type);
            }

            return r;
        }
    }
}
