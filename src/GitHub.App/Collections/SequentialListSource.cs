using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GitHub.Logging;
using GitHub.Models;
using ReactiveUI;
using Serilog;

namespace GitHub.Collections
{
    /// <summary>
    /// An <see cref="IVirtualizingListSource{T}"/> that loads GraphQL pages sequentially, and
    /// transforms items into a view model after reading.
    /// </summary>
    /// <typeparam name="TModel">The type of the model read from the remote data source.</typeparam>
    /// <typeparam name="TViewModel">The type of the transformed view model.</typeparam>
    /// <remarks>
    /// GraphQL can only read pages of data sequentally, so in order to read item 450 (assuming a
    /// page size of 100), the list source must read pages 1, 2, 3 and 4 in that order. Classes
    /// deriving from this class only need to implement <see cref="LoadPage(string)"/> to load a
    /// single page and this class will handle the rest.
    /// 
    /// In addition, items will usually need to be transformed into a view model after reading. The
    /// implementing class overrides <see cref="CreateViewModel(TModel)"/> to carry out that
    /// transformation.
    /// </remarks>
    public abstract class SequentialListSource<TModel, TViewModel> : ReactiveObject, IVirtualizingListSource<TViewModel>
    {
        static readonly ILogger log = LogManager.ForContext<SequentialListSource<TModel, TViewModel>>();

        readonly Dispatcher dispatcher;
        readonly object loadLock = new object();
        Dictionary<int, Page<TModel>> pages = new Dictionary<int, Page<TModel>>();
        Task loading = Task.CompletedTask;
        bool disposed;
        bool isLoading;
        int? count;
        int nextPage;
        int loadTo;
        string after;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialListSource{TModel, TViewModel}"/> class.
        /// </summary>
        public SequentialListSource()
        {
            dispatcher = Application.Current?.Dispatcher;
        }

        /// <inheritdoc/>
        public bool IsLoading
        {
            get { return isLoading; }
            private set { this.RaiseAndSetIfChanged(ref isLoading, value); }
        }

        /// <inheritdoc/>
        public virtual int PageSize => 100;

        event EventHandler PageLoaded;

        public void Dispose() => disposed = true;

        /// <inheritdoc/>
        public async Task<int> GetCount()
        {
            dispatcher?.VerifyAccess();

            if (!count.HasValue)
            {
                count = (await EnsureLoaded(0).ConfigureAwait(false)).TotalCount;
            }

            return count.Value;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<TViewModel>> GetPage(int pageNumber)
        {
            dispatcher?.VerifyAccess();

            var page = await EnsureLoaded(pageNumber);

            if (page == null)
            {
                return null;
            }

            var result = page.Items
                .Select(CreateViewModel)
                .ToList();
            pages.Remove(pageNumber);
            return result;
        }

        /// <summary>
        /// When overridden in a derived class, transforms a model into a view model after loading.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The view model.</returns>
        protected abstract TViewModel CreateViewModel(TModel model);

        /// <summary>
        /// When overridden in a derived class reads a page of results from GraphQL.
        /// </summary>
        /// <param name="after">The GraphQL after cursor.</param>
        /// <returns>A task which returns the page of results.</returns>
        protected abstract Task<Page<TModel>> LoadPage(string after);

        /// <summary>
        /// Called when the source begins loading pages.
        /// </summary>
        protected virtual void OnBeginLoading()
        {
            IsLoading = true;
        }

        /// <summary>
        /// Called when the source finishes loading pages.
        /// </summary>
        protected virtual void OnEndLoading()
        {
            IsLoading = false;
        }

        async Task<Page<TModel>> EnsureLoaded(int pageNumber)
        {
            if (pageNumber < nextPage)
            {
                return pages[pageNumber];
            }

            var pageLoaded = WaitPageLoaded(pageNumber);
            loadTo = Math.Max(loadTo, pageNumber);

            while (!disposed)
            {
                lock (loadLock)
                {
                    if (loading.IsCompleted)
                    {
                        loading = Load();
                    }
                }

                var completed = await Task.WhenAny(loading, pageLoaded).ConfigureAwait(false);

                if (completed.IsFaulted)
                {
                    throw completed.Exception;
                }

                if (pageLoaded.IsCompleted)
                {
                    // A previous waiting task may have already returned the page. If so, return null.
                    pages.TryGetValue(pageNumber, out var result);
                    return result;
                }
            }

            return null;
        }

        Task WaitPageLoaded(int page)
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler handler = null;
            handler = (s, e) =>
            {
                if (nextPage > page)
                {
                    tcs.SetResult(true);
                    PageLoaded -= handler;
                }
            };
            PageLoaded += handler;
            return tcs.Task;
        }

        async Task Load()
        {
            OnBeginLoading();

            try
            {
                while (nextPage <= loadTo && !disposed)
                {
                    await LoadNextPage().ConfigureAwait(false);
                    PageLoaded?.Invoke(this, EventArgs.Empty);
                }
            }
            finally
            {
                OnEndLoading();
            }
        }

        async Task LoadNextPage()
        {
            log.Debug("Loading page {Number} of {ModelType}", nextPage, typeof(TModel));

            var page = await LoadPage(after).ConfigureAwait(false);
            pages[nextPage++] = page;
            after = page.EndCursor;
        }
    }
}