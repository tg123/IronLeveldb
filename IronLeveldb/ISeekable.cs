using System.Collections.Generic;

namespace IronLevelDB
{
    public interface ISeekable<in TK, out TV>
    {
        IEnumerable<TV> Seek(TK key);

        IEnumerable<TV> SeekFirst();

        // TODO support seeklast and reverse direction
    }
}