using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace IronLeveldb.Cache.LRU
{
    public class LruCache : ICache
    {
        private readonly long _capacity;

        private readonly ConcurrentDictionary<ByteArrayKey, Node>
            _data = new ConcurrentDictionary<ByteArrayKey, Node>();

        private readonly Node _head;

        private readonly ReaderWriterLockSlim _rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly Node _tail;

        private long _used;

        public LruCache(long capacity)
        {
            _capacity = capacity;

            _head = new Node();
            _tail = new Node();

            _head.Next = _tail;
            _tail.Prev = _head;
        }


        public void Insert<T>(long namespaceId, IReadOnlyList<byte> key, T value)
        {
            var charge = (value as IChargeValue)?.Charge ?? 1;

            if (charge > _capacity)
            {
                throw new OverflowException();
            }

            var node = new Node<T>
            {
                ByteArrayKey = new ByteArrayKey(namespaceId, key),
                Charge = charge,
                Value = value
            };

            _rwlock.EnterWriteLock();

            try
            {
                // make space for newbie
                while (_used + charge > _capacity)
                {
                    var leastUsed = _tail.Prev;

                    if (_data.TryRemove(leastUsed.ByteArrayKey, out leastUsed))
                    {
                        RemoveFromLinkedList(leastUsed);
                        _used -= leastUsed.Charge;
                    }
                }

                // add newbie
                _data.AddOrUpdate(node.ByteArrayKey, node, (_, old) =>
                {
                    RemoveFromLinkedList(old);
                    _used -= old.Charge;

                    return node;
                });

                AddAtHead(node);
                _used += charge;
            }
            finally
            {
                _rwlock.ExitWriteLock();
            }
        }

        public T Lookup<T>(long namespaceId, IReadOnlyList<byte> key)
        {
            _rwlock.EnterUpgradeableReadLock();

            try
            {
                if (!_data.TryGetValue(new ByteArrayKey(namespaceId, key), out var t))
                {
                    return default(T);
                }

                if (!(t is Node<T> node))
                {
                    return default(T);
                }

                MoveToHead(node);

                return node.Value;
            }
            finally
            {
                _rwlock.ExitUpgradeableReadLock();
            }
        }

        public void Erase(long namespaceId, IReadOnlyList<byte> key)
        {
            if (_data.TryRemove(new ByteArrayKey(namespaceId, key), out var removed))
            {
                _rwlock.EnterWriteLock();
                try
                {
                    RemoveFromLinkedList(removed);
                    _used -= removed.Charge;
                }
                finally
                {
                    _rwlock.ExitWriteLock();
                }
            }
        }

        public void Prune()
        {
            _rwlock.EnterWriteLock();

            try
            {
                _head.Next = _tail;
                _tail.Prev = _head;

                _data.Clear();
                _used = 0;
            }
            finally
            {
                _rwlock.ExitWriteLock();
            }
        }

        private void MoveToHead(Node node)
        {
            _rwlock.EnterWriteLock();
            try
            {
                RemoveFromLinkedList(node);
                AddAtHead(node);
            }
            finally
            {
                _rwlock.ExitWriteLock();
            }
        }

        private static void RemoveFromLinkedList(Node node)
        {
            var prev = node.Prev;
            var next = node.Next;

            prev.Next = next;
            next.Prev = prev;
        }

        // should lock
        private void AddAtHead(Node node)
        {
            var current = _head.Next;

            // set head
            _head.Next = node;
            node.Prev = _head;

            node.Next = current;
            current.Prev = node;
        }

        private class Node
        {
            public ByteArrayKey ByteArrayKey { get; set; }
            public Node Next { get; set; }
            public Node Prev { get; set; }
            public long Charge { get; set; }
        }

        private class Node<T> : Node
        {
            public T Value { get; set; }
        }
    }
}
