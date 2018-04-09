using System;
using System.IO;

namespace IronLeveldb
{
    public interface IIronLeveldbStorge : IDisposable
    {
        IContentReader GetCurrentDescriptorContent();

        IContentReader GetTableContentById(ulong num);
    }
}
