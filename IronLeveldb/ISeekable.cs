using System.Collections.Generic;

namespace IronLeveldb
{
    public interface ISeekable<in TK, out TV>
    {
        IEnumerable<TV> Seek(TK key);

        IEnumerable<TV> SeekFirst();

        // TODO support seeklast and reverse direction
    }
}
