using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GitHub.Logging;
using GitHub.Models;
using Serilog;

namespace GitHub.Collections
{
    public abstract class SequentialListSource<TModel, TViewModel> : IVirtualizingListSource<TViewModel>
    {
        static readonly ILogger log = LogManager.ForContext<SequentialListSource<TModel, TViewModel>>();

        readonly CancellationToken cancel;
        readonly Dispatcher dispatcher;
        readonly object loadLock = new object();
        Dictionary<int, Page<TModel>> pages = new Dictionary<int, Page<TModel>>();
        Task loading = Task.CompletedTask;
        int? count;
        int nextPage;
        int loadTo;
        string after;

        public SequentialListSource(CancellationToken cancel)
        {
            this.cancel = cancel;
            dispatcher = Application.Current.Dispatcher;
        }

        public virtual int PageSize => 100;
        event EventHandler PageLoaded;

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
        }

        protected virtual void OnEndLoading()
        {
        }

        async Task<Page<TModel>> EnsureLoaded(int pageNumber)
        {
            if (pageNumber < nextPage)
            {
                return pages[pageNumber];
            }

            var pageLoaded = WaitPageLoaded(pageNumber);
            loadTo = Math.Max(loadTo, pageNumber);

            while (!cancel.IsCancellationRequested)
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

            while (nextPage <= loadTo && !cancel.IsCancellationRequested)
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