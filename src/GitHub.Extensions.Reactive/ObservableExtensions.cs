using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;

namespace GitHub.Extensions.Reactive
{
    public static class ObservableExtensions
    {
        public static IObservable<T> WhereNotNull<T>(this IObservable<T> observable) where T : class
        {
            return observable.Where(item => item != null);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Rx has your back.")]
        public static IObservable<T> PublishAsync<T>(this IObservable<T> observable)
        {
            var ret = observable.Multicast(new AsyncSubject<T>());
            ret.Connect();
            return ret;
        }

        /// <summary>
        /// Used in cases where we only care when an observable completes and not what the observed values are.
        /// This is commonly used by some of our "Refresh" methods.
        /// </summary>
        public static IObservable<Unit> AsCompletion<T>(this IObservable<T> observable)
        {
            return observable
                .SelectMany(_ => Observable.Empty<Unit>())
                .Concat(Observable.Return(Unit.Default));
        }

        /// <summary>
        /// Subscribes to and produces values from the observable returned 
        /// by <paramref name="selector"/> once the source <paramref name="observable"/> 
        /// has completed while ignoring all elements produced from the source.
        /// </summary>
        /// <typeparam name="T">The type of the source observable</typeparam>
        /// <typeparam name="TRet">The type of the resulting observable.</typeparam>
        /// <param name="observable">The source observable.</param>
        /// <param name="selector">The selector for producing the return observable.</param>
        public static IObservable<TRet> ContinueAfter<T, TRet>(this IObservable<T> observable, Func<IObservable<TRet>> selector)
        {
            return observable.AsCompletion().SelectMany(_ => selector());
        }

        /// <summary>
        /// Helper method to transform an IObservable{Unit} to IObservable{object} with a null value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"></param>
        /// <returns></returns>
        public static IObservable<object> SelectNull(this IObservable<Unit> observable)
        {
            return observable.Select(_ => (object)null);
        }

        /// <summary>
        /// Helper method to transform an IObservable{T} to IObservable{Unit}.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"></param>
        /// <returns></returns>
        public static IObservable<Unit> SelectUnit<T>(this IObservable<T> observable)
        {
            return observable.Select(_ => Unit.Default);
        }

        public static IObservable<TSource> CatchNonCritical<TSource>(
            this IObservable<TSource> first,
            Func<Exception, IObservable<TSource>> second)
        {
            return first.Catch<TSource, Exception>(e =>
            {
                if (!e.IsCriticalException())
                    return second(e);

                return Observable.Throw<TSource>(e);
            });
        }

        public static IObservable<TSource> CatchNonCritical<TSource>(
            this IObservable<TSource> first,
            IObservable<TSource> second)
        {
            return first.CatchNonCritical(e => second);
        }

        /// <summary>
        /// An exponential back off strategy which starts with 1 second and then 4, 9, 16...
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly Func<int, TimeSpan> ExpontentialBackoff = n => TimeSpan.FromSeconds(Math.Pow(n, 2));

        /// <summary>
        /// Returns a cold observable which retries (re-subscribes to) the source observable on error up to the 
        /// specified number of times or until it successfully terminates. Allows for customizable back off strategy.
        /// </summary>
        /// <param name="source">The source observable.</param>
        /// <param name="retryCount">The number of attempts of running the source observable before failing.</param>
        /// <param name="strategy">The strategy to use in backing off, exponential by default.</param>
        /// <param name="retryOnError">A predicate determining for which exceptions to retry. Defaults to all</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <returns>
        /// A cold observable which retries (re-subscribes to) the source observable on error up to the 
        /// specified number of times or until it successfully terminates.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static IObservable<T> RetryWithBackoffStrategy<T>(
            this IObservable<T> source,
            int retryCount = 3,
            Func<int, TimeSpan> strategy = null,
            Func<Exception, bool> retryOnError = null,
            IScheduler scheduler = null)
        {
            strategy = strategy ?? ExpontentialBackoff;
            scheduler = scheduler ?? RxApp.TaskpoolScheduler;

            if (retryOnError == null)
                retryOnError = e => e.CanRetry();

            int attempt = 0;

            return Observable.Defer(() =>
            {
                return ((++attempt == 1) ? source : source.DelaySubscription(strategy(attempt - 1), scheduler))
                    .Select(item => new Tuple<bool, T, Exception>(true, item, null))
                    .Catch<Tuple<bool, T, Exception>, Exception>(e => retryOnError(e)
                        ? Observable.Throw<Tuple<bool, T, Exception>>(e)
                        : Observable.Return(new Tuple<bool, T, Exception>(false, default(T), e)));
            })
            .Retry(retryCount)
            .SelectMany(t => t.Item1
                ? Observable.Return(t.Item2)
                : Observable.Throw<T>(t.Item3));
        }

        /// <summary>
        /// Returns an observable sequence that terminates with an exception if the source observable
        /// doesn't produce any values. If the source observable produces values or errors those are
        /// forwarded transparently.
        /// </summary>
        /// <param name="source">The source observable.</param>
        /// <param name="exc">The exception to use if the source observable doesn't produce a value.</param>
        public static IObservable<T> ErrorIfEmpty<T>(this IObservable<T> source, Exception exc)
        {
            return source
                .Materialize()
                .Scan(Tuple.Create<bool, Notification<T>>(false, null),
                    (prev, cur) => Tuple.Create(prev.Item1 || cur.Kind == NotificationKind.OnNext, cur))
                .SelectMany(x => !x.Item1 && x.Item2.Kind == NotificationKind.OnCompleted
                    ? Observable.Throw<Notification<T>>(exc)
                    : Observable.Return(x.Item2))
                .Dematerialize();
        }

        /// <summary>
        /// Flattens an observable of enumerables into a stream of individual signals.
        /// </summary>
        /// <remarks>
        /// I end up doing this a lot and it looks cleaner. Note that this could produce bad results if you expect
        /// the observable to return a single collection and it returns multiple collections.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<T> Flatten<T>(this IObservable<IEnumerable<T>> source)
        {
            return source.SelectMany(items => items);
        }

        /// <summary>
        /// Aggregates the items in the observable sequence into a readonly list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<IReadOnlyList<T>> ToReadOnlyList<T>(this IObservable<T> source)
        {
            return source.ToList().Select(list => new ReadOnlyCollection<T>(list));
        }

        /// <summary>
        /// Aggregates the items in the observable sequence into a readonly list.
        /// </summary>
        public static IObservable<IReadOnlyList<TResult>> ToReadOnlyList<T, TResult>(this IObservable<IEnumerable<T>> source, Func<T, TResult> map)
        {
            return source.Select(items => new ReadOnlyCollection<TResult>(items.Select(map).ToList()));
        }

        /// <summary>
        /// Aggregates the items in the observable sequence into a readonly list.
        /// </summary>
        public static IObservable<IReadOnlyList<TResult>> ToReadOnlyList<T, TResult>(this IObservable<IEnumerable<T>> source, Func<T, TResult> map, TResult firstItem)
        {
            return source.Select(items => new ReadOnlyCollection<TResult>(new[] { firstItem }.Concat(items.Select(map)).ToList()));
        }
    }
}
