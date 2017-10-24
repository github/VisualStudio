using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using Akavache;
using GitHub.Extensions;

namespace GitHub.Caches
{
    public class CacheIndex
    {
        public const string PRPrefix = "index:pr";
        public const string RepoPrefix = "index:repos";
        public const string GitIgnoresPrefix = "index:ignores";
        public const string LicensesPrefix = "index:licenses";
        public const string FileContentsPrefix = "index:filecontents";

        public static CacheIndex Create(string key)
        {
            return new CacheIndex { IndexKey = key };
        }

        public CacheIndex()
        {
            Keys = new List<string>();
            OldKeys = new List<string>();
        }

        public CacheIndex Add(string indexKey, CacheItem item)
        {
            Guard.ArgumentNotEmptyString(indexKey, nameof(indexKey));
            Guard.ArgumentNotNull(item, nameof(item));

            var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", IndexKey, item.Key);
            if (!Keys.Contains(k))
                Keys.Add(k);
            UpdatedAt = DateTimeOffset.UtcNow;
            return this;
        }

        public IObservable<CacheIndex> AddAndSave(IBlobCache cache, string indexKey, CacheItem item,
            DateTimeOffset? absoluteExpiration = null)
        {
            Guard.ArgumentNotNull(cache, nameof(cache));
            Guard.ArgumentNotEmptyString(indexKey, nameof(indexKey));
            Guard.ArgumentNotNull(item, nameof(item));

            var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", IndexKey, item.Key);
            if (!Keys.Contains(k))
                Keys.Add(k);
            UpdatedAt = DateTimeOffset.UtcNow;
            return cache.InsertObject(IndexKey, this, absoluteExpiration)
                .Select(x => this);
        }

        public static IObservable<CacheIndex> AddAndSaveToIndex(IBlobCache cache, string indexKey, CacheItem item,
            DateTimeOffset? absoluteExpiration = null)
        {
            Guard.ArgumentNotNull(cache, nameof(cache));
            Guard.ArgumentNotEmptyString(indexKey, nameof(indexKey));
            Guard.ArgumentNotNull(item, nameof(item));

            return cache.GetOrCreateObject(indexKey, () => Create(indexKey))
                .Do(index =>
                {
                    var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", index.IndexKey, item.Key);
                    if (!index.Keys.Contains(k))
                        index.Keys.Add(k);
                    index.UpdatedAt = DateTimeOffset.UtcNow;
                })
                .SelectMany(index => cache.InsertObject(index.IndexKey, index, absoluteExpiration)
                .Select(x => index));
        }

        public CacheIndex Clear()
        {
            OldKeys = Keys.ToList();
            Keys.Clear();
            UpdatedAt = DateTimeOffset.UtcNow;
            return this;
        }

        public IObservable<CacheIndex> Save(IBlobCache cache,
            DateTimeOffset? absoluteExpiration = null)
        {
            Guard.ArgumentNotNull(cache, nameof(cache));

            return cache.InsertObject(IndexKey, this, absoluteExpiration)
                .Select(x => this);
        }

        public string IndexKey { get; set; }
        public List<string> Keys { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<string> OldKeys { get; set; }
    }
}
