using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace GitHub.Services
{
    /// <summary>
    /// Serializes and shares subscriptions to observables based on a key.
    /// 
    /// If two callers simultaneously request an observable for a particular
    /// key only one of the calls will end up creating the observable. Both
    /// callers will then share a single (replying) connection to the created
    /// observable.
    /// 
    /// This is useful for scenarios where you want to ensure that multiple
    /// callers to a long-running observable and/or expensive observable operation
    /// share a single subscription.
    /// 
    /// As soon as all subscribers to the observable have disconnected the
    /// provider opens up a slot to produce a new observable for the given key.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the key, this will be passed to the factory and should
    /// as such be something through which the factory can produce an observable
    /// .</typeparam>
    /// <typeparam name="TValue">
    /// The type of the value which will be produced by the observable 
    /// returned by <see cref="Get"/>.
    /// </typeparam>
    /// <remarks>
    /// You might be wondering how this differs from ObservableEx.CurrentOrCreate.
    /// It does so in a few ways. For one it returns cold observables that are
    /// reference counted. This means that you can use this with observables that 
    /// produce a never-ending stream of values. The observable will be kept
    /// alive until there are no more subscribers. Secondly the CurrentOrCreate
    /// function shares a single global dictionary for all observables in an
    /// app whereas this can be used as an instance variable in the class
    /// requiring serialization. Further this provider guarantees that the
    /// same factory is used for all Get operations whereas CurrentOrCreate
    /// allows for different factories which may lead to unexpected results.
    /// </remarks>
    public class SerializedObservableProvider<TKey, TValue>
    {
        readonly ConcurrentDictionary<TKey, IObservable<TValue>> dictionary;
        readonly Func<TKey, IObservable<TValue>> factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedObservableProvider{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="factory">
        /// The factory which, given a key, will produce the observable that is then
        /// shared for concurrent callers of <see cref="Get"/>.
        /// </param>
        public SerializedObservableProvider(Func<TKey, IObservable<TValue>> factory)
        {
            this.factory = factory;
            this.dictionary = new ConcurrentDictionary<TKey, IObservable<TValue>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedObservableProvider{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="factory">
        /// The factory which, given a key, will produce the observable that is then
        /// shared for concurrent callers of <see cref="Get" />.</param>
        /// <param name="comparer">
        /// The comparer to use when determining if there's already an observable running
        /// for the given key.
        /// </param>
        public SerializedObservableProvider(Func<TKey, IObservable<TValue>> factory, IEqualityComparer<TKey> comparer)
        {
            this.factory = factory;
            this.dictionary = new ConcurrentDictionary<TKey, IObservable<TValue>>(comparer);
        }

        /// <summary>
        /// Gets an observable for the specified key. If there's an observable already running for the
        /// given key the caller will receive a shared subscription to that observable which will replay
        /// any values produced up to that point.
        /// </summary>
        public IObservable<TValue> Get(TKey key)
        {
            // This outer defer is here to allow for subsequent subscriptions. If a caller gets an
            // observable from this method, subscribes to it, disposes the subscription and then
            // subscribes again this will stop the caller from subscribing to a potentially stale
            // observable.
            return Observable.Defer(() =>
            {
                return dictionary.GetOrAdd(key, _ =>
                {
                    // The order of these operations are important. The finally needs to act on
                    // the _single_ subscription from RefCount. Moving it last in this order will
                    // cause us to remove the observable for each call.
                    return Observable.Defer(() => EvalFactorySafe(key, factory))
                        // NB. There's a narrow race condition here. If caller A holds a
                        // subscription and caller B attempt to get an observable
                        // for the same key caller B may get the observable while caller A unsubscribes
                        // from it. This will lead to caller B re-subscribing to the observable even
                        // though a third caller could be retrieving a new observable for the same key.
                        // For our use of this method that's not a big deal. There's (at least) two
                        // possible solutions to this. One is to only remove the observable from the
                        // dictionary when it completes (dangerous for never-ending observables) and the
                        // other is to serialize the dispose method of the subscription on a scheduler.
                        .Finally(() => TryRemove(key))
                        .Replay()
                        .RefCount();
                });
            });
        }

        static IObservable<TValue> EvalFactorySafe(TKey key, Func<TKey, IObservable<TValue>> factory)
        {
            try
            {
                return factory(key);
            }
            catch (Exception exc)
            {
                return Observable.Throw<TValue>(exc);
            }
        }

        bool TryRemove(TKey key)
        {
            IObservable<TValue> ignore;
            return dictionary.TryRemove(key, out ignore);
        }
    }
}