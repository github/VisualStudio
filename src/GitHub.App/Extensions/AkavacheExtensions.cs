using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Akavache;
using GitHub.Caches;
using System.Threading.Tasks;

namespace GitHub.Extensions
{
    public static class AkavacheExtensions
    {
        /// <summary>
        /// This method attempts to returned a cached value, in the case of a
        /// cache miss the fetchFunc will be used to provide a fresh value which
        /// is then returned. In the case of a cache hit where the cache item is
        /// considered stale (but not expired) as determined by <paramref name="refreshInterval"/>
        /// the stale value will be produced first, followed by the fresh value
        /// when the fetch func completes.
        /// </summary>
        /// <param name="blobCache">The cache to retrieve the object from.</param>
        /// <param name="key">The key to look up the cache value with.</param>
        /// <param name="fetchFunc">The fetch function.</param>
        /// <param name="refreshInterval">
        /// Cache objects with an age exceeding this value will be treated as stale
        /// and the fetch function will be invoked to refresh it.
        /// </param>
        /// <param name="maxCacheDuration">
        /// The maximum age of a cache object before the object is treated as
        /// expired and unusable. Cache objects older than this will be treated
        /// as a cache miss.
        /// </param>
        public static IObservable<T> GetAndRefreshObject<T>(
            this IBlobCache blobCache,
            string key,
            Func<IObservable<T>> fetchFunc,
            TimeSpan refreshInterval,
            TimeSpan maxCacheDuration)
        {
            return Observable.Defer(() =>
            {
                var absoluteExpiration = blobCache.Scheduler.Now + maxCacheDuration;

                try
                {
                    return blobCache.GetAndFetchLatest(
                        key,
                        fetchFunc,
                        createdAt => IsExpired(blobCache, createdAt, refreshInterval),
                        absoluteExpiration);
                }
                catch (Exception exc)
                {
                    return Observable.Throw<T>(exc);
                }
            });
        }

        /// <summary>
        /// This method attempts to returned a cached value, in the case of a
        /// cache miss the fetchFunc will be used to provide a fresh value which
        /// is then returned. In the case of a cache hit where the cache item is
        /// considered stale (but not expired) as determined by <paramref name="refreshInterval"/>
        /// the stale value will be produced first, followed by the fresh value
        /// when the fetch func completes.
        /// </summary>
        /// <param name="blobCache">The cache to retrieve the object from.</param>
        /// <param name="key">The key to look up the cache value with.</param>
        /// <param name="fetchFunc">The fetch function.</param>
        /// <param name="refreshInterval">
        /// Cache objects with an age exceeding this value will be treated as stale
        /// and the fetch function will be invoked to refresh it.
        /// </param>
        /// <param name="maxCacheDuration">
        /// The maximum age of a cache object before the object is treated as
        /// expired and unusable. Cache objects older than this will be treated
        /// as a cache miss.
        /// </param>
        public static IObservable<byte[]> GetAndRefresh(
            this IBlobCache blobCache,
            string key,
            Func<IObservable<byte[]>> fetchFunc,
            TimeSpan refreshInterval,
            TimeSpan maxCacheDuration)
        {
            return Observable.Defer(() =>
            {
                var absoluteExpiration = blobCache.Scheduler.Now + maxCacheDuration;

                try
                {
                    return blobCache.GetAndFetchLatestBytes(
                        key,
                        fetchFunc,
                        createdAt => IsExpired(blobCache, createdAt, refreshInterval),
                        absoluteExpiration);
                }
                catch (Exception exc)
                {
                    return Observable.Throw<byte[]>(exc);
                }
            });
        }

        /// <summary>
        /// This is the non-generic analog of JsonSerializationMixin.GetAndFetchLatest[1]
        /// We shouldn't make modifications to this that alter its behavior from the generic
        /// version. By having this we can keep our two GetAndRefresh methods extremely
        /// similar and thus trust that what works in one will work in the other.
        /// 
        /// 1. https://github.com/akavache/Akavache/blob/1b19bb56d/Akavache/Portable/JsonSerializationMixin.cs#L202-L236
        /// </summary>
        static IObservable<byte[]> GetAndFetchLatestBytes(
            this IBlobCache blobCache,
            string key,
            Func<IObservable<byte[]>> fetchFunc,
            Func<DateTimeOffset, bool> fetchPredicate = null,
            DateTimeOffset? absoluteExpiration = null)
        {
            var fetch = Observable.Defer(() => blobCache.GetCreatedAt(key))
                .Select(x => fetchPredicate == null || x == null || fetchPredicate(x.Value))
                .Where(predicateIsTrue => predicateIsTrue)
                .SelectMany(_ =>
                {
                    return fetchFunc()
                        .SelectMany(x => blobCache.Invalidate(key).Select(__ => x))
                        .SelectMany(x => blobCache.Insert(key, x, absoluteExpiration).Select(__ => x));
                });

            var result = blobCache.Get(key).Select(x => Tuple.Create(x, true))
                .Catch(Observable.Return(new Tuple<byte[], bool>(null, false)));

            return result
                .SelectMany(x => x.Item2
                    ? Observable.Return(x.Item1)
                    : Observable.Empty<byte[]>())
                .Concat(fetch)
                .Replay()
                .RefCount();
        }

        /// <summary>
        /// This method attempts to return a series of cached value(s) aggregated by
        /// a <paramref name="key"/>. In the case of a
        /// cache miss the fetchFunc will be used to provide a fresh value which
        /// is then returned. In the case of a cache hit where the cache item is
        /// considered stale (but not expired) as determined by <paramref name="refreshInterval"/>
        /// the stale values will be produced first, followed by the fresh values
        /// when the fetch func completes.
        /// </summary>
        /// <param name="blobCache">The cache to retrieve the object from.</param>
        /// <param name="key">The key to look up the cache value with.</param>
        /// <param name="fetchFunc">The fetch function.</param>
        /// <param name="removedItemsCallback">The callback that receives items that
        /// were a part of the cache but not of the list list of values.</param>
        /// <param name="refreshInterval">
        /// Cache objects with an age exceeding this value will be treated as stale
        /// and the fetch function will be invoked to refresh it.
        /// </param>
        /// <param name="maxCacheDuration">
        /// The maximum age of a cache object before the object is treated as
        /// expired and unusable. Cache objects older than this will be treated
        /// as a cache miss.
        /// </param>
        public static IObservable<T> GetAndFetchLatestFromIndex<T>(
            this IBlobCache blobCache,
            string key,
            Func<IObservable<T>> fetchFunc,
            Action<T> removedItemsCallback,
            TimeSpan refreshInterval,
            TimeSpan maxCacheDuration)
                where T : CacheItem
        {
            return Observable.Defer(() =>
            {
                var absoluteExpiration = blobCache.Scheduler.Now + maxCacheDuration;

                try
                {
                    return blobCache.GetAndFetchLatestFromIndex(
                        key,
                        fetchFunc,
                        removedItemsCallback,
                        createdAt => IsExpired(blobCache, createdAt, refreshInterval),
                        absoluteExpiration);
                }
                catch (Exception exc)
                {
                    return Observable.Throw<T>(exc);
                }
            });
        }

        static IObservable<T> GetAndFetchLatestFromIndex<T>(this IBlobCache This,
            string key,
            Func<IObservable<T>> fetchFunc,
            Action<T> removedItemsCallback,
            Func<DateTimeOffset, bool> fetchPredicate = null,
            DateTimeOffset? absoluteExpiration = null,
            bool shouldInvalidateOnError = false)
                where T : CacheItem
        {
            var idx = Observable.Defer(() => This.GetOrCreateObject(key, () => CacheIndex.Create(key))).Replay().RefCount();


            var fetch = idx
                .Select(x => Tuple.Create(x, fetchPredicate == null || !x.Keys.Any() || fetchPredicate(x.UpdatedAt)))
                .Where(predicateIsTrue => predicateIsTrue.Item2)
                .Select(x => x.Item1)
                .Select(index => index.Clear())
                .SelectMany(index => fetchFunc()
                        .Catch<T, Exception>(ex =>
                        {
                            var shouldInvalidate = shouldInvalidateOnError ?
                                This.InvalidateObject<CacheIndex>(key) :
                                Observable.Return(Unit.Default);
                            return shouldInvalidate.SelectMany(__ => Observable.Throw<T>(ex));
                        })
                        .SelectMany(x => x.Save<T>(This, key, absoluteExpiration))
                        .Do(x => index.Add(key, x))
                );

            var cache = idx
                .SelectMany(index => This.GetObjects<T>(index.Keys.ToList()))
                .SelectMany(dict => dict.Values);

            return cache.Merge(fetch)
                .Finally(async () =>
                {
                    var index = await idx;
                    await index.Save(This);

                    var list = index.OldKeys.Except(index.Keys);
                    if (!list.Any())
                        return;
                    var removed = await This.GetObjects<T>(list);
                    foreach (var d in removed.Values)
                        removedItemsCallback(d);
                    await This.InvalidateObjects<T>(list);
                })
                .Replay().RefCount();
        }

        /// <summary>
        /// This method adds a new object to the database and updates the
        /// corresponding index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="blobCache">The cache to retrieve the object from.</param>
        /// <param name="key">The key to look up the cache value with.</param>
        /// <param name="item">The item to add to the database</param>
        /// <param name="maxCacheDuration">
        /// The maximum age of a cache object before the object is treated as
        /// expired and unusable. Cache objects older than this will be treated
        /// as a cache miss.
        /// <returns></returns>
        public static IObservable<T> PutAndUpdateIndex<T>(this IBlobCache blobCache,
            string key,
            Func<IObservable<T>> fetchFunc,
            TimeSpan maxCacheDuration)
            where T : CacheItem
        {
            return Observable.Defer(() =>
            {
                var absoluteExpiration = blobCache.Scheduler.Now + maxCacheDuration;
                return blobCache.GetOrCreateObject(key, () => CacheIndex.Create(key))
                    .SelectMany(index => fetchFunc()
                            .Catch<T, Exception>(Observable.Throw<T>)
                            .SelectMany(x => x.Save<T>(blobCache, key, absoluteExpiration))
                            .Do(x => index.AddAndSave(blobCache, key, x, absoluteExpiration))
                    );
            });
        }

        static bool IsExpired(IBlobCache blobCache, DateTimeOffset itemCreatedAt, TimeSpan cacheDuration)
        {
            var elapsed = blobCache.Scheduler.Now - itemCreatedAt.ToUniversalTime();

            return elapsed > cacheDuration;
        }
    }
}