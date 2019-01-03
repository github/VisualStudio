using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Helpers;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.Cache
{
    public abstract class AutoCompleteSourceCache : IAutoCompleteSourceCache
    {
        static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        
        readonly SerializedObservableProvider<IRepositoryModel, IReadOnlyList<SuggestionItem>> serializedSuggestions;
        readonly TimeSpan cacheDuration;
        readonly TimeSpan maxCacheDuration;

        protected AutoCompleteSourceCache(TimeSpan cacheDuration, TimeSpan maxCacheDuration)
        {
            this.cacheDuration = cacheDuration;
            this.maxCacheDuration = maxCacheDuration;

            serializedSuggestions = new SerializedObservableProvider<IRepositoryModel, IReadOnlyList<SuggestionItem>>(
                GetAndFetchCachedSourceItemsImpl);
        }

        // We append this to the cache key to differentiate the various auto completion caches.
        protected abstract string CacheSuffix { get; }

        /// <summary>
        /// Retrieves suggestions from the cache for the specified repository. If not there, it makes an API
        /// call to  retrieve them.
        /// </summary>
        /// <param name="repository">The repository that contains the suggestion items.</param>
        /// <returns>An observable containing a readonly list of auto complete suggestions</returns>
        public IObservable<IReadOnlyList<SuggestionItem>> RetrieveSuggestions(IRepositoryModel repository)
        {
            Guard.ArgumentNotNull(repository, "repository");

            return serializedSuggestions.Get(repository);
        }

        /// <summary>
        /// Calls the API to fetch mentionables, issues, or whatever.
        /// </summary>
        /// <param name="existingCachedItems">Existing items in the cache. Useful for incremental cache updates</param>
        /// <param name="repository">The repository containing the items to fetch</param>
        /// <param name="apiClient">The API client to use to make the request</param>
        protected abstract IObservable<SuggestionItem> FetchSuggestionsSourceItems(
            CachedData<List<SuggestionItem>> existingCachedItems,
            IRepositoryModel repository,
            IApiClient apiClient);

        IObservable<List<SuggestionItem>> GetAndFetchExistingCachedSourceItems(
            IHostCache hostCache,
            IRepositoryModel repository,
            Func<IRepositoryModel, CachedData<List<SuggestionItem>>, IObservable<List<SuggestionItem>>> fetchFunc)
        {
            Debug.Assert(repository != null, "Repository cannot be null because we validated it at the callsite");
            Debug.Assert(hostCache != null, "HostCache cannot be null because we validated it at the callsite");

            return GetOrFetchCachedItems(hostCache, repository, fetchFunc);
        }

        IObservable<IReadOnlyList<SuggestionItem>> GetAndFetchCachedSourceItemsImpl(IRepositoryModel repository)
        {
            Debug.Assert(repository != null, "Repository cannot be null because we validated it at the callsite");
            
            return Observable.Defer(() =>
            {
                var hostCache = repository.RepositoryHost != null
                    ? repository.RepositoryHost.Cache
                    : null;

                if (hostCache == null)
                {
                    return Observable.Empty<IReadOnlyList<SuggestionItem>>();
                }

                var ret = new ReplaySubject<List<SuggestionItem>>(1);

                GetAndFetchExistingCachedSourceItems(hostCache, repository, GetCacheableSuggestions)
                    .Catch<List<SuggestionItem>, Exception>(_ => Observable.Return(new List<SuggestionItem>()))
                    .Multicast(ret)
                    .PermaRef();

                // If GetAndFetchExistingCachedSourceItems finds that the cache item is stale it produces the
                // stale value and then fetches and produces a fresh one. It can thus produce
                // 1, 2 or no values (if cache miss and fetch fails). While I'd ideally want
                // to expose that through this method so that the suggestions list in the UI get updated
                // as soon as we have a fresh value this method has historically only produced
                // one value so in an effort to reduce scope I'm keeping it that way. We
                // unfortunately still need to maintain the subscription to GetAndRefresh though
                // so that we don't cancel the refresh as soon as we get the stale object.
                return ret.Take(1);
            });
        }

        IObservable<List<SuggestionItem>> GetCacheableSuggestions(
            IRepositoryModel repository,
            CachedData<List<SuggestionItem>> existingCachedItems)
        {
            Debug.Assert(repository != null, "Repository cannot be null because we validated it at the callsite");

            var apiClient = repository.RepositoryHost != null ? repository.RepositoryHost.ApiClient : null;
            if (apiClient == null)
            {
                return Observable.Empty<List<SuggestionItem>>();
            }

            // Our current serializer can't handle deserializing IReadOnlyList<T>. That's why we need a concrete list
            // here.
            return FetchSuggestionsSourceItems(existingCachedItems, repository, apiClient)
                .ToConcreteList();
        }

        IObservable<List<T>> GetOrFetchCachedItems<T>(
            IHostCache hostCache,
            IRepositoryModel repositoryModel,
            Func<IRepositoryModel, CachedData<List<T>>, IObservable<List<T>>> fetchFunc)
            where T : class
        {
            Ensure.ArgumentNotNull(repositoryModel, "repositoryModel");
            Ensure.ArgumentNotNull(fetchFunc, "fetchFunc");

            string cacheKey = repositoryModel.NameWithOwner + ":" + CacheSuffix;
            return hostCache.LocalMachine.GetCachedValueThenFetchForNextTime<List<T>>(
                cacheKey,
                cacheData => fetchFunc(repositoryModel, cacheData),
                cacheDuration,
                maxCacheDuration)
                .Catch<List<T>, Exception>(ex =>
                {
                    log.Info(String.Format(CultureInfo.InvariantCulture,
                        "Exception occurred attempting to get a cached value and then fetch '{0}'", cacheKey), ex);
                    return Observable.Return(new List<T>());
                })
                .Select(result => result ?? new List<T>());
        }
    }
}
