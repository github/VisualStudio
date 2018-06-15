using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Extensions;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    public abstract class IssueListViewModelBase : PanePageViewModelBase, IIssueListViewModelBase
    {
        CancellationTokenSource cancel;
        IReadOnlyList<IIssueListItemViewModelBase> items;
        ICollectionView itemsView;
        string searchQuery;
        string selectedState;
        string stringFilter;
        int numberFilter;

        public IssueListViewModelBase()
        {
            OpenItem = ReactiveCommand.CreateAsyncTask(OpenItemImpl);
        }

        public IReadOnlyList<IIssueListItemViewModelBase> Items
        {
            get { return items; }
            private set { this.RaiseAndSetIfChanged(ref items, value); }
        }

        public ICollectionView ItemsView
        {
            get { return itemsView; }
            private set { this.RaiseAndSetIfChanged(ref itemsView, value); }
        }

        public ILocalRepositoryModel LocalRepository { get; private set; }

        public string SearchQuery
        {
            get { return searchQuery; }
            set { this.RaiseAndSetIfChanged(ref searchQuery, value); }
        }

        public string SelectedState
        {
            get { return selectedState; }
            set { this.RaiseAndSetIfChanged(ref selectedState, value); }
        }

        public abstract IReadOnlyList<string> States { get; }

        public ReactiveCommand<Unit> OpenItem { get; }

        public async Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            LocalRepository = repository;
            SelectedState = States.FirstOrDefault();
            this.WhenAnyValue(x => x.SelectedState).Skip(1).Subscribe(_ => Refresh().Forget());
            this.WhenAnyValue(x => x.SearchQuery).Skip(1).Subscribe(_ => FilterChanged());
            await Refresh();
        }

        public override Task Refresh()
        {
            cancel?.Cancel();
            cancel?.Dispose();
            cancel = new CancellationTokenSource();

            var items = new VirtualizingList<IIssueListItemViewModelBase>(CreateItemSource(cancel.Token), null);
            Items = items;
            ItemsView = new VirtualizingListCollectionView<IIssueListItemViewModelBase>(items);
            ItemsView.Filter = FilterItem;
            return Task.CompletedTask;
        }

        protected abstract IVirtualizingListSource<IIssueListItemViewModelBase> CreateItemSource(CancellationToken cancel);
        protected abstract Task DoOpenItem(IViewModel item);

        void FilterChanged()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                numberFilter = 0;

                if (SearchQuery.StartsWith('#'))
                {
                    int.TryParse(SearchQuery.Substring(1), out numberFilter);
                }

                if (numberFilter == 0)
                {
                    stringFilter = SearchQuery.ToUpper();
                }
            }
            else
            {
                stringFilter = null;
                numberFilter = 0;
            }

            ItemsView?.Refresh();
        }

        bool FilterItem(object o)
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var item = o as IIssueListItemViewModelBase;

                if (item != null)
                {
                    if (numberFilter != 0)
                    {
                        return item.Number == numberFilter;
                    }
                    else
                    {
                        return item.Title.ToUpper().Contains(stringFilter);
                    }
                }
            }

            return true;
        }

        async Task OpenItemImpl(object i)
        {
            var item = i as IViewModel;
            if (item != null) await DoOpenItem(item);
        }
    }
}
