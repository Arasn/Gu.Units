namespace Gu.Units
{
    using System;
    using System.Collections.Generic;

    internal class SubstringCache<TItem>
    {
        private static readonly CachedItem[] Empty = new CachedItem[0];
        private readonly object gate = new object();
        private CachedItem[] cache = Empty;

        internal bool TryGetBySubString(string text, int pos, out CachedItem result)
        {
            if (text == null)
            {
                result = default(CachedItem);
                return false;
            }

            var tempCache = this.cache;
            var index = BinaryFindSubstring(this.cache, text, pos);
            if (index < 0)
            {
                result = default(CachedItem);
                return false;
            }

            result = tempCache[index];
            for (int i = index + 1; i < tempCache.Length; i++)
            {
                // searching linearly for longest match after finding one
                var temp = tempCache[i];
                if (Compare(temp.Key, text, pos) == 0)
                {
                    result = temp;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }

        internal bool TryGet(string key, out CachedItem match)
        {
            if (key == null)
            {
                match = default(CachedItem);
                return false;
            }

            var tempCache = this.cache;
            var index = BinaryFind(this.cache, key);
            if (index < 0)
            {
                match = default(CachedItem);
                return false;
            }

            match = tempCache[index];
            return true;
        }

        internal CachedItem Add(string key, TItem item)
        {
            Ensure.NotNull(key, $"{nameof(item)}.{key}");

            lock (this.gate) // this was five times faster than ReaderWriterLockSlim in benchmarks.
            {
                var i = BinaryFind(this.cache, key);
                if (i >= 0)
                {
                    var cachedItem = this.cache[i];
                    if (cachedItem.Key == key)
                    {
                        if (Equals(cachedItem.Value, item))
                        {
                            return cachedItem;
                        }

                        throw new InvalidOperationException("Cannot add same key with different values.\r\n" +
                                                            $"The key is {key} and the values are {{{item}, {cachedItem.Value}}}");
                    }
                }

                var updated = new CachedItem[this.cache.Length + 1];
                Array.Copy(this.cache, 0, updated, 0, this.cache.Length);
                var newItem = new CachedItem(key, item);
                updated[this.cache.Length] = newItem;
                Array.Sort(updated);
                this.cache = updated;
                return newItem;
            }
        }

        private static int BinaryFindSubstring(CachedItem[] cache, string key, int pos)
        {
            int lo = 0;
            int hi = cache.Length - 1;
            while (lo <= hi)
            {
                var i = (lo + hi) / 2;
                var cached = cache[i];
                var c = Compare(cached.Key, key, pos);
                if (c == 0)
                {
                    return i;
                }

                if (c < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        private static int BinaryFind(CachedItem[] cache, string key)
        {
            int lo = 0;
            int hi = cache.Length - 1;
            while (lo <= hi)
            {
                var i = (lo + hi) / 2;
                var cached = cache[i];
                var c = Compare(cached.Key, key);
                if (c == 0)
                {
                    return i;
                }

                if (c < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        private static int Compare(string cached, string key, int pos)
        {
            for (int i = 0; i < cached.Length; i++)
            {
                var j = i + pos;
                if (key.Length == j)
                {
                    return 1;
                }

                var compare = cached[i] - key[j];
                if (compare != 0)
                {
                    return compare;
                }
            }

            return 0;
        }

        private static int Compare(string cached, string key)
        {
            for (int i = 0; i < cached.Length; i++)
            {
                if (key.Length == i)
                {
                    return 1;
                }

                var compare = cached[i] - key[i];
                if (compare != 0)
                {
                    return compare;
                }
            }

            return cached.Length - key.Length;
        }

        internal struct CachedItem : IComparable<CachedItem>
        {
            internal readonly string Key;
            internal readonly TItem Value;

            public CachedItem(string key, TItem value)
            {
                this.Key = key;
                this.Value = value;
            }

            public int CompareTo(CachedItem other)
            {
                return Compare(this.Key, other.Key);
            }

            public override string ToString() => this.Key;
        }
    }
}