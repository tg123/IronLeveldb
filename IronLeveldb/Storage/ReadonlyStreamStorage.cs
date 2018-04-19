using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IronLeveldb.Storage
{
    public abstract class ReadonlyStreamStorage : IIronLeveldbStorge
    {
        private readonly ConcurrentStack<WeakReference<IDisposable>> _opened =
            new ConcurrentStack<WeakReference<IDisposable>>();

        private volatile bool _disposed;

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            var cs = new List<Exception>();
            for (;;)
            {
                if (_opened.IsEmpty)
                {
                    if (cs.Count > 1)
                    {
                        throw new AggregateException(cs);
                    }

                    if (cs.Count == 1)
                    {
                        throw cs.First();
                    }

                    break;
                }

                if (!_opened.TryPop(out var reference))
                {
                    continue;
                }

                if (reference.TryGetTarget(out var obj))
                {
                    try
                    {
                        obj.Dispose();
                    }
                    catch (Exception e)
                    {
                        cs.Add(e);
                    }
                }
            }
        }

        public virtual IContentReader GetCurrentDescriptorContent()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ReadonlyStreamStorage));
            }

            return new StreamContentReader(DeferForDispose(GetCurrentDescriptorContentStream()));
        }

        public virtual IContentReader GetTableContentById(ulong num)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ReadonlyStreamStorage));
            }

            return new StreamContentReader(DeferForDispose(GetTableContentStreamById(num)));
        }

        protected T DeferForDispose<T>(T obj) where T : IDisposable
        {
            _opened.Push(new WeakReference<IDisposable>(obj));
            return obj;
        }

        protected abstract Stream GetCurrentDescriptorContentStream();

        protected abstract Stream GetTableContentStreamById(ulong num);
    }
}
