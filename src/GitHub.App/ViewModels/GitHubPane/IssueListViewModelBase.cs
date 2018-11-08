using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using GitHub.Collections;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model which implements the functionality common to issue and pull request lists.
    /// </summary>
    public abstract class IssueListViewModelBase : PanePageViewModelBase, IIssueListViewModelBase
    {
        static readonly ILogger log = LogManager.ForContext<GitHubPaneViewModel>();
        readonly IRepositoryService repositoryService;
        IReadOnlyList<IIssueListItemViewModelBase> items;
        ICollectionView itemsView;
        IDisposable subscription;
        IssueListMessage message;
        RepositoryModel remoteRepository;
        IReadOnlyList<RepositoryModel> forks;
        string searchQuery;
        string selectedState;
        ObservableAsPropertyHelper<string> stateCaption;
        string stringFilter;
        int numberFilter;
        IUserFilterViewModel authorFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueListViewModelBase"/> class.
        /// </summary>
        /// <param name="repositoryService">The repository service.</param>
        public IssueListViewModelBase(IRepositoryService repositoryService)
        {
            this.repositoryService = repositoryService;
            OpenItem = ReactiveCommand.CreateFromTask<IIssueListItemViewModelBase>(OpenItemImpl);
            stateCaption = this.WhenAnyValue(
                x => x.Items.Count,
                x => x.SelectedState,
                x => x.IsBusy,
                x => x.IsLoading,
                (count, state, busy, loading) => busy || loading ? state : count + " " + state)
                .ToProperty(this, x => x.StateCaption);
        }

        /// <inheritdoc/>
        public IUserFilterViewModel AuthorFilter
        {
            get { return authorFilter; }
            private set { this.RaiseAndSetIfChanged(ref authorFilter, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<RepositoryModel> Forks
        {
            get { return forks; }
            set { this.RaiseAndSetIfChanged(ref forks, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<IIssueListItemViewModelBase> Items
        {
            get { return items; }
            private set { this.RaiseAndSetIfChanged(ref items, value); }
        }

        /// <inheritdoc/>
        public ICollectionView ItemsView
        {
            get { return itemsView; }
            private set { this.RaiseAndSetIfChanged(ref itemsView, value); }
        }

        /// <inheritdoc/>
        public LocalRepositoryModel LocalRepository { get; private set; }

        /// <inheritdoc/>
        public IssueListMessage Message
        {
            get { return message; }
            private set { this.RaiseAndSetIfChanged(ref message, value); }
        }

        /// <inheritdoc/>
        public RepositoryModel RemoteRepository
        {
            get { return remoteRepository; }
            set { this.RaiseAndSetIfChanged(ref remoteRepository, value); }
        }

        /// <inheritdoc/>
        public string SearchQuery
        {
            get { return searchQuery; }
            set { this.RaiseAndSetIfChanged(ref searchQuery, value); }
        }

        /// <inheritdoc/>
        public string SelectedState
        {
            get { return selectedState; }
            set { this.RaiseAndSetIfChanged(ref selectedState, value); }
        }

        /// <inheritdoc/>
        public abstract IReadOnlyList<string> States { get; }

        /// <inheritdoc/>
        public string StateCaption => stateCaption.Value;

        /// <inheritdoc/>
        public ReactiveCommand<IIssueListItemViewModelBase, Unit> OpenItem { get; }

        /// <inheritdoc/>
        public async Task InitializeAsync(LocalRepositoryModel repository, IConnection connection)
        {
            try
            {
                LocalRepository = repository;
                SelectedState = States.FirstOrDefault();
                AuthorFilter = new UserFilterViewModel(LoadAuthors);
                IsLoading = true;

                var parent = await repositoryService.FindParent(
                    HostAddress.Create(repository.CloneUrl),
                    repository.Owner,
                    repository.Name);

                if (parent == null)
                {
                    RemoteRepository = repository;
                }
                else
                {
                    // TODO: Handle forks with different names.
                    RemoteRepository = new RepositoryModel(
                        repository.Name,
                        UriString.ToUriString(repository.CloneUrl.ToRepositoryUrl(parent.Value.owner)));

                    Forks = new RepositoryModel[]
                    {
                    RemoteRepository,
                    repository,
                    };
                }

                this.WhenAnyValue(x => x.SelectedState, x => x.RemoteRepository)
                    .Skip(1)
                    .Subscribe(_ => Refresh().Forget());

                Observable.Merge(
                    this.WhenAnyValue(x => x.SearchQuery).Skip(1).SelectUnit(),
                    AuthorFilter.WhenAnyValue(x => x.Selected).Skip(1).SelectUnit())
                    .Subscribe(_ => FilterChanged());

                await Refresh();
            }
            catch (Exception ex)
            {
                Error = ex;
                IsLoading = false;
                log.Error(ex, "Error initializing IssueListViewModelBase");
            }
        }

        /// <summary>
        /// Refreshes the view model.
        /// </summary>
        /// <returns>A task tracking the operation.</returns>
        public override Task Refresh()
        {
            if (RemoteRepository == null)
            {
                // If an exception occurred reading the parent repository, do nothing.
                return Task.CompletedTask;
            }

            subscription?.Dispose();

            var dispose = new CompositeDisposable();
            var itemSource = CreateItemSource();
            var items = new VirtualizingList<IIssueListItemViewModelBase>(itemSource, null);
            var view = new VirtualizingListCollectionView<IIssueListItemViewModelBase>(items);

            view.Filter = FilterItem;
            Items = items;
            ItemsView = view;
            Error = null;

            dispose.Add(itemSource);
            dispose.Add(
                Observable.CombineLatest(
                    itemSource.WhenAnyValue(x => x.IsLoading),
                    view.WhenAnyValue(x => x.Count),
                    this.WhenAnyValue(x => x.SearchQuery),
                    this.WhenAnyValue(x => x.SelectedState),
                    this.WhenAnyValue(x => x.AuthorFilter.Selected),
                    (loading, count, _, __, ___) => Tuple.Create(loading, count))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => UpdateState(x.Item1, x.Item2)));
            dispose.Add(
                Observable.FromEventPattern<ErrorEventArgs>(
                    x => items.InitializationError += x,
                    x => items.InitializationError -= x)
                .Subscribe(x => Error = x.EventArgs.GetException()));
            subscription = dispose;

            return Task.CompletedTask;
        }

        /// <summary>
        /// When overridden in a derived class, creates the <see cref="IVirtualizingListSource{T}"/>
        /// that will act as the source for <see cref="Items"/>.
        /// </summary>
        protected abstract IVirtualizingListSource<IIssueListItemViewModelBase> CreateItemSource();

        /// <summary>
        /// When overridden in a derived class, navigates to the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>A task tracking the operation.</returns>
        protected abstract Task DoOpenItem(IIssueListItemViewModelBase item);

        /// <summary>
        /// Loads a page of authors for the <see cref="AuthorFilter"/>.
        /// </summary>
        /// <param name="after">The GraphQL "after" cursor.</param>
        /// <returns>A task that returns a page of authors.</returns>
        protected abstract Task<Page<ActorModel>> LoadAuthors(string after);

        void FilterChanged()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                numberFilter = 0;

                int.TryParse(SearchQuery.Substring(SearchQuery.StartsWith('#') ? 1 : 0), out numberFilter);

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
            var item = o as IIssueListItemViewModelBase;
            var result = true;

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {

                if (item != null)
                {
                    if (numberFilter != 0)
                    {
                        result = item.Number == numberFilter;
                    }
                    else
                    {
                        result = item.Title.ToUpper().Contains(stringFilter);
                    }
                }
            }

            if (result && AuthorFilter.Selected != null)
            {
                result = item.Author.Login.Equals(
                    AuthorFilter.Selected.Login,
                    StringComparison.CurrentCultureIgnoreCase);
            }

            return result;
        }

        async Task OpenItemImpl(IIssueListItemViewModelBase item)
        {
            if (item != null) await DoOpenItem(item);
        }

        void UpdateState(bool loading, int count)
        {
            var message = IssueListMessage.None;

            if (!loading)
            {
                if (count == 0)
                {
                    if (SelectedState == States[0] &&
                        string.IsNullOrWhiteSpace(SearchQuery) &&
                        AuthorFilter.Selected == null)
                    {
                        message = IssueListMessage.NoOpenItems;
                    }
                    else
                    {
                        message = IssueListMessage.NoItemsMatchCriteria;
                    }
                }

                IsLoading = false;
            }

            IsBusy = loading;
            Message = message;
        }
    }
}
