using Akavache;
using NullGuard;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Caches
{
    public class CacheIndex
    {
        public static CacheIndex Create(string key)
        {
            return new CacheIndex() { IndexKey = key };
        }

        public CacheIndex()
        {
            Keys = new List<string>();
        }

        public IObservable<CacheIndex> AddAndSave(IBlobCache cache, string indexKey, CacheItem item,
            DateTimeOffset? absoluteExpiration = null)
        {
            var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", IndexKey, item.Key);
            if (!Keys.Contains(k))
                Keys.Add(k);
            UpdatedAt = DateTimeOffset.Now;
            return cache.InsertObject(IndexKey, this, absoluteExpiration)
                        .Select(x => this);
        }

        public static IObservable<CacheIndex> AddAndSaveToIndex(IBlobCache cache, string indexKey, CacheItem item,
            DateTimeOffset? absoluteExpiration = null)
        {
            return cache.GetObject<CacheIndex>(indexKey)
                .Do(index =>
                {
                    var k = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", index.IndexKey, item.Key);
                    if (!index.Keys.Contains(k))
                        index.Keys.Add(k);
                    index.UpdatedAt = DateTimeOffset.Now;
                })
                .SelectMany(index => cache.InsertObject(index.IndexKey, index, absoluteExpiration)
                .Select(x => index));
        }

        [AllowNull]
        public string IndexKey {[return: AllowNull] get; set; }
        public List<string> Keys { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
