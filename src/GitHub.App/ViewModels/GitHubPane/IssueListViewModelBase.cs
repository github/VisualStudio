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
        IReadOnlyList<IViewModel> items;
        ICollectionView itemsView;
        string searchQuery;
        string selectedState;

        public IssueListViewModelBase()
        {
            OpenItem = ReactiveCommand.CreateAsyncTask(OpenItemImpl);
        }

        public IReadOnlyList<IViewModel> Items
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
            this.WhenAnyValue(x => x.SelectedState).Skip(1).Subscribe(_ => Load().Forget());
            await Load();
        }

        protected abstract IVirtualizingListSource<IViewModel> CreateItemSource();
        protected abstract Task DoOpenItem(IViewModel item);

        Task Load()
        {
            var items = new VirtualizingList<IViewModel>(CreateItemSource(), null);
            Items = items;
            ItemsView = new VirtualizingListCollectionView<IViewModel>(items);

            return Task.CompletedTask;
        }

        async Task OpenItemImpl(object i)
        {
            var item = i as IViewModel;
            if (item != null) await DoOpenItem(item);
        }
    }
}
