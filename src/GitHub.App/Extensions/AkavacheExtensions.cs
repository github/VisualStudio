using System;
using System.Reactive.Linq;
using Akavache;

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

        static bool IsExpired(IBlobCache blobCache, DateTimeOffset itemCreatedAt, TimeSpan cacheDuration)
        {
            var now = blobCache.Scheduler.Now;
            var elapsed = now - itemCreatedAt;

            return elapsed > cacheDuration;
        }
    }
}