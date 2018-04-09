using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronLeveldb.DB;
using IronLeveldb.SSTable;
using IronLeveldb.Storage;
using IronLeveldb.Storage.FileSystem;
using Version = IronLeveldb.DB.Version;

namespace IronLeveldb
{
    public static class IronLeveldbBuilder
    {
        public static IIronLeveldb Build(this IIronLeveldbStorge storge)
        {
            return Build(storge, new DefaultIronLeveldbOptions());
        }

        public static IIronLeveldb Build(this IIronLeveldbStorge storge, IIronLeveldbOptions options)
        {
            var manifestContent = storge.GetCurrentDescriptorContent();

            // TODO base version
            var b = new Version.Builder(storge, options, null);
            using (var manifestStream = manifestContent.AsStream())
            {
                foreach (var stream in new RecoverLogRecordsStream(manifestStream))
                {
                    var versionEdit = VersionEdit.DecodeFrom(stream);

                    b.With(versionEdit);
                }
            }

            var current = b.Build();
            return new IronLeveldbStub(options, current, () =>
            {
                var es = new List<Exception>();
                try
                {
                    storge.Dispose();
                }
                catch (Exception e)
                {
                    es.Add(e);
                }

                try
                {
                    current.Dispose();
                }
                catch (Exception e)
                {
                    es.Add(e);
                }

                if (es.Count == 2)
                {
                    throw new AggregateException(es);
                }

                if (es.Count == 1)
                {
                    throw es.First();
                }
            });
        }

        public static IIronLeveldb BuildFromPath(string path, IIronLeveldbOptions options)
        {
            return Build(new ReadonlyFileSystemStorage(path), options);
        }

        public static IIronLeveldb BuildFromPath(string path)
        {
            return BuildFromPath(path, new DefaultIronLeveldbOptions());
        }


        public static IIronLeveldb BuildFromSingleTable(Stream stream, IIronLeveldbOptions options)
        {
            var table = new Table(new StreamContentReader(stream), options.BlockCache,
                new InternalKeyComparer(options.Comparer));
            return new IronLeveldbStub(options, table, stream.Dispose);
        }

        public static IIronLeveldb BuildFromSingleTable(Stream stream)
        {
            return BuildFromSingleTable(stream, new DefaultIronLeveldbOptions());
        }

        public static IIronLeveldb BuildFromSingleTable(Stream stream, ICache blockCache)
        {
            return BuildFromSingleTable(stream, new DefaultIronLeveldbOptions
            {
                BlockCache = blockCache
            });
        }
    }
}
