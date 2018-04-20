using System;
using System.Collections.Generic;
using System.Linq;

namespace IronLeveldb
{
    public interface IIronLeveldb : ISeekable<IReadOnlyList<byte>, IReadonlyBytesKeyValuePair>, IDisposable
    {
        // TODO Put
    }

    public static class IronLeveldbExt
    {
        public static IReadOnlyList<byte> Get(this IIronLeveldb db, IReadOnlyList<byte> key)
        {
            return Get(db, key, ReadOptions.Default);
        }

        public static IReadOnlyList<byte> Get(this IIronLeveldb db, IReadOnlyList<byte> key, ReadOptions options)
        {
            var v = db.Seek(key, options).FirstOrDefault();

            var sk = v?.Key;

            if (sk?.Count == key.Count)
            {
                for (var i = key.Count - 1; i >= 0; i--)
                {
                    if (key[i] != sk[i])
                    {
                        return null;
                    }
                }

                return v.Value;
            }

            return null;
        }
    }
}
