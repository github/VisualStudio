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

        public SequentialListSource()
        {
            dispatcher = Application.Current.Dispatcher;
        }

        public bool IsLoading
        {
            get { return isLoading; }
            private set { this.RaiseAndSetIfChanged(ref isLoading, value); }
        }

        public virtual int PageSize => 100;

        event EventHandler PageLoaded;

        public void Dispose() => disposed = true;

        public async Task<int> GetCount()
        {
            dispatcher.VerifyAccess();

            if (!count.HasValue)
            {
                count = (await EnsureLoaded(0).ConfigureAwait(false)).TotalCount;
            }

            return count.Value;
        }

        public async Task<IReadOnlyList<TViewModel>> GetPage(int pageNumber)
        {
            dispatcher.VerifyAccess();

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

        protected abstract TViewModel CreateViewModel(TModel model);
        protected abstract Task<Page<TModel>> LoadPage(string after);

        protected virtual void OnBeginLoading()
        {
            IsLoading = true;
        }

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

                await Task.WhenAny(loading, pageLoaded).ConfigureAwait(false);

                if (pageLoaded.IsCompleted)
                {
                    return pages[pageNumber];
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

            while (nextPage <= loadTo && !disposed)
            {
                await LoadNextPage().ConfigureAwait(false);
                PageLoaded?.Invoke(this, EventArgs.Empty);
            }

            OnEndLoading();
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