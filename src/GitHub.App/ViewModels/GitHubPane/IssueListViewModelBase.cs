using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
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
        IReadOnlyList<IIssueListItemViewModelBase> items;
        ICollectionView itemsView;
        IDisposable subscription;
        IssueListMessage message;
        string searchQuery;
        string selectedState;
        string stringFilter;
        int numberFilter;
        IUserFilterViewModel authorFilter;

        public IssueListViewModelBase()
        {
            OpenItem = ReactiveCommand.CreateAsyncTask(OpenItemImpl);
        }

        public IUserFilterViewModel AuthorFilter
        {
            get { return authorFilter; }
            private set { this.RaiseAndSetIfChanged(ref authorFilter, value); }
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

        public IssueListMessage Message
        {
            get { return message; }
            private set { this.RaiseAndSetIfChanged(ref message, value); }
        }

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
            AuthorFilter = new UserFilterViewModel("Author", LoadAuthors);
            this.WhenAnyValue(x => x.SelectedState).Skip(1).Subscribe(_ => Refresh().Forget());
            this.WhenAnyValue(x => x.SearchQuery).Skip(1).Subscribe(_ => FilterChanged());
            IsLoading = true;
            await Refresh();
        }

        public override Task Refresh()
        {
            subscription?.Dispose();

            var dispose = new CompositeDisposable();
            var itemSource = CreateItemSource();
            var items = new VirtualizingList<IIssueListItemViewModelBase>(itemSource, null);
            var view = new VirtualizingListCollectionView<IIssueListItemViewModelBase>(items);

            view.Filter = FilterItem;
            Items = items;
            ItemsView = view;

            dispose.Add(itemSource);
            dispose.Add(
                Observable.CombineLatest(
                    itemSource.WhenAnyValue(x => x.IsLoading),
                    view.WhenAnyValue(x => x.Count),
                    (loading, count) => Tuple.Create(loading, count))
                .Subscribe(x => UpdateState(x.Item1, x.Item2)));
            subscription = dispose;

            return Task.CompletedTask;
        }

        protected abstract IVirtualizingListSource<IIssueListItemViewModelBase> CreateItemSource();
        protected abstract Task DoOpenItem(IViewModel item);
        protected abstract Task<Page<ActorModel>> LoadAuthors(string after);

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

        void UpdateState(bool loading, int count)
        {
            var message = IssueListMessage.None;

            if (!loading)
            {
                if (count == 0)
                {
                    message = SelectedState == States[0] && string.IsNullOrWhiteSpace(SearchQuery) ?
                        IssueListMessage.NoOpenItems :
                        IssueListMessage.NoItemsMatchCriteria;
                }

                IsLoading = false;
            }

            IsBusy = loading;
            Message = message;
        }
    }
}
