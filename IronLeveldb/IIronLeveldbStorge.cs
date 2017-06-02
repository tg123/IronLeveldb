using System;
using System.IO;

namespace IronLevelDB
{
    public interface IIronLeveldbStorge : IDisposable
    {
        Stream GetCurrentDescriptorContent();

        Stream GetDescriptorContentById(ulong num);

        IContentReader GetTableContentById(ulong num);
    }
}