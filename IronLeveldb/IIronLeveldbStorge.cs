using System;

namespace IronLeveldb
{
    public interface IIronLeveldbStorge : IDisposable
    {
        IContentReader GetCurrentDescriptorContent();

        IContentReader GetTableContentById(ulong num);
    }
}
