using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LevelDB
{
    /// <summary>
    /// Minimal P/Invoke bindings to the real native leveldb C API (<c>leveldb/c.h</c>).
    /// This provides a small, cross-platform (Linux/macOS/Windows) replacement for the
    /// Windows-only <c>LevelDB.Net</c> NuGet package, so the end-to-end tests can create
    /// genuine leveldb databases on any platform and then read them back with IronLeveldb.
    /// </summary>
    internal static class Native
    {
        private const string Library = "leveldb";

        static Native()
        {
            NativeLibrary.SetDllImportResolver(typeof(Native).Assembly, Resolve);
        }

        private static IntPtr Resolve(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != Library)
            {
                return IntPtr.Zero;
            }

            string[] candidates;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                candidates = new[] { "leveldb.dll", "libleveldb.dll" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                candidates = new[] { "libleveldb.dylib", "libleveldb.1.dylib" };
            }
            else
            {
                candidates = new[] { "libleveldb.so", "libleveldb.so.1", "libleveldb.so.1d" };
            }

            foreach (var candidate in candidates)
            {
                if (NativeLibrary.TryLoad(candidate, out var handle))
                {
                    return handle;
                }
            }

            return IntPtr.Zero;
        }

        // make sure the static ctor (resolver) runs before any DllImport call.
        internal static void EnsureInitialized()
        {
        }

        [DllImport(Library)]
        internal static extern IntPtr leveldb_options_create();

        [DllImport(Library)]
        internal static extern void leveldb_options_destroy(IntPtr options);

        [DllImport(Library)]
        internal static extern void leveldb_options_set_create_if_missing(IntPtr options, byte value);

        [DllImport(Library)]
        internal static extern void leveldb_options_set_write_buffer_size(IntPtr options, UIntPtr size);

        [DllImport(Library)]
        internal static extern IntPtr leveldb_open(IntPtr options,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string name, out IntPtr errptr);

        [DllImport(Library)]
        internal static extern void leveldb_close(IntPtr db);

        [DllImport(Library)]
        internal static extern IntPtr leveldb_writeoptions_create();

        [DllImport(Library)]
        internal static extern void leveldb_writeoptions_destroy(IntPtr options);

        [DllImport(Library)]
        internal static extern void leveldb_put(IntPtr db, IntPtr options,
            byte[] key, UIntPtr keylen, byte[] val, UIntPtr vallen, out IntPtr errptr);

        [DllImport(Library)]
        internal static extern void leveldb_delete(IntPtr db, IntPtr options,
            byte[] key, UIntPtr keylen, out IntPtr errptr);

        [DllImport(Library)]
        internal static extern void leveldb_free(IntPtr ptr);

        internal static void ThrowIfError(IntPtr errptr)
        {
            if (errptr == IntPtr.Zero)
            {
                return;
            }

            var message = Marshal.PtrToStringAnsi(errptr);
            leveldb_free(errptr);
            throw new LevelDBException(message);
        }
    }

    public class LevelDBException : Exception
    {
        public LevelDBException(string message) : base(message)
        {
        }
    }

    public class Options
    {
        public bool CreateIfMissing { get; set; }

        public ulong WriteBufferSize { get; set; }
    }

    public class WriteOptions
    {
    }

    /// <summary>
    /// A thin, write-focused wrapper over the native leveldb database, mirroring the
    /// small surface of <c>LevelDB.Net</c> that the IronLeveldb tests rely on.
    /// </summary>
    public sealed class DB : IDisposable
    {
        private IntPtr _handle;

        private DB(IntPtr handle)
        {
            _handle = handle;
        }

        public static DB Open(string path, Options options)
        {
            Native.EnsureInitialized();

            var opts = Native.leveldb_options_create();
            try
            {
                Native.leveldb_options_set_create_if_missing(opts, (byte)(options.CreateIfMissing ? 1 : 0));
                if (options.WriteBufferSize > 0)
                {
                    // clamp to platform pointer size to avoid overflow on the native size_t.
                    var size = options.WriteBufferSize > (ulong)UIntPtr.MaxValue
                        ? UIntPtr.MaxValue
                        : (UIntPtr)options.WriteBufferSize;
                    Native.leveldb_options_set_write_buffer_size(opts, size);
                }

                var handle = Native.leveldb_open(opts, path, out var err);
                Native.ThrowIfError(err);
                return new DB(handle);
            }
            finally
            {
                Native.leveldb_options_destroy(opts);
            }
        }

        public void Put(WriteOptions writeOptions, string key, string value)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var valueBytes = Encoding.UTF8.GetBytes(value);
            var opts = Native.leveldb_writeoptions_create();
            try
            {
                Native.leveldb_put(_handle, opts, keyBytes, (UIntPtr)keyBytes.Length,
                    valueBytes, (UIntPtr)valueBytes.Length, out var err);
                Native.ThrowIfError(err);
            }
            finally
            {
                Native.leveldb_writeoptions_destroy(opts);
            }
        }

        public void Delete(WriteOptions writeOptions, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var opts = Native.leveldb_writeoptions_create();
            try
            {
                Native.leveldb_delete(_handle, opts, keyBytes, (UIntPtr)keyBytes.Length, out var err);
                Native.ThrowIfError(err);
            }
            finally
            {
                Native.leveldb_writeoptions_destroy(opts);
            }
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                Native.leveldb_close(_handle);
                _handle = IntPtr.Zero;
            }
        }
    }
}
