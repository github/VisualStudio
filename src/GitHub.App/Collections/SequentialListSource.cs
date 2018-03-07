using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Logging;
using GitHub.Models;
using Serilog;

namespace GitHub.Collections
{
    public abstract class SequentialListSource<TModel, TViewModel> : IVirtualizingListSource<TViewModel>
    {
        static readonly ILogger log = LogManager.ForContext<SequentialListSource<TModel, TViewModel>>();

        readonly SemaphoreSlim loading = new SemaphoreSlim(1);
        Dictionary<int, Page<TModel>> pages;
        int count;
        int nextPage;
        string after;

        public virtual int PageSize => 100;

        public async Task<int> GetCount()
        {
            if (pages == null)
            {
                await LoadNextPage().ConfigureAwait(false);
            }

            return count;
        }

        public async Task<IList<TViewModel>> GetPage(int pageNumber)
        {
            await LoadTo(pageNumber);

            var result = pages[pageNumber].Items
                .Select(CreateViewModel)
                .ToList();

            pages[pageNumber] = null;
            return result;
        }

        protected abstract TViewModel CreateViewModel(TModel model);
        protected abstract Task<Page<TModel>> LoadPage(string after);

        protected virtual void OnBeginLoading(int nextPage, int toPage)
        {
        }

        protected virtual void OnEndLoading()
        {
        }

        async Task LoadTo(int pageNumber)
        {
            await loading.WaitAsync().ConfigureAwait(false);

            try
            {
                var needsLoad = nextPage <= pageNumber;

                if (needsLoad)
                {
                    OnBeginLoading(nextPage, pageNumber);
                }

                while (nextPage <= pageNumber)
                {
                    await LoadNextPage().ConfigureAwait(false);
                }

                if (needsLoad)
                {
                    OnEndLoading();
                }
            }
            finally
            {
                loading.Release();
            }
        }

        async Task LoadNextPage()
        {
            log.Debug("Loading page {Number} of {ModelType}", nextPage, typeof(TModel));

            var page = await LoadPage(after).ConfigureAwait(false);

            if (pages == null)
            {
                pages = new Dictionary<int, Page<TModel>>();
                count = page.TotalCount;
            }

            pages[nextPage++] = page;
            after = page.EndCursor;
        }
    }
}
