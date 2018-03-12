using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Collections;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IIssueListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssueListViewModel : PanePageViewModelBase, IIssueListViewModel
    {
        static readonly string NoFilter = "[None]";
        static readonly string[] NoFilterList = new[] { NoFilter };

        readonly IVisualStudioBrowser visualStudioBrowser;
        readonly IIssueService service;
        CancellationTokenSource cancelLoad;
        IReadOnlyList<IIssueListItemViewModel> issues;
        ICollectionView issuesView;
        IUserFilterViewModel authorFilter;
        IUserFilterViewModel assigneeFilter;
        string searchQuery;
        string selectedAssignee;
        IssueStateFilter selectedState = IssueStateFilter.Open;

        [ImportingConstructor]
        public IssueListViewModel(
            IIssueService service,
            IVisualStudioBrowser visualStudioBrowser)
        {
            this.service = service;
            this.visualStudioBrowser = visualStudioBrowser;

            States = new[] { IssueStateFilter.Open, IssueStateFilter.Closed, IssueStateFilter.All };
            Title = Resources.IssuesNavigationItemText;

            this.WhenAnyValue(
                x => x.SelectedState,
                x => x.AuthorFilter.Selected)
                .Subscribe(_ => issuesView?.Refresh());

            OpenIssueOnGitHub = ReactiveCommand.Create()
                .OnExecuteCompleted(x => DoOpenIssueOnGitHub((int)x));
        }

        public IReadOnlyList<IssueStateFilter> States { get; }
        public ILocalRepositoryModel LocalRepository { get; private set; }

        public IUserFilterViewModel AuthorFilter
        {
            get { return authorFilter; }
            private set { this.RaiseAndSetIfChanged(ref authorFilter, value); }
        }

        public IUserFilterViewModel AssigneeFilter
        {
            get { return assigneeFilter; }
            private set { this.RaiseAndSetIfChanged(ref assigneeFilter, value); }
        }

        public IReadOnlyList<IIssueListItemViewModel> Issues
        {
            get { return issues; }
            private set { this.RaiseAndSetIfChanged(ref issues, value); }
        }

        public ICollectionView IssuesView
        {
            get { return issuesView; }
            private set { this.RaiseAndSetIfChanged(ref issuesView, value); }
        }

        public string SearchQuery
        {
            get { return searchQuery; }
            set { this.RaiseAndSetIfChanged(ref searchQuery, value); }
        }

        public string SelectedAssignee
        {
            get { return selectedAssignee; }
            set { this.RaiseAndSetIfChanged(ref selectedAssignee, CastFilter(value)); }
        }

        public IssueStateFilter SelectedState
        {
            get { return selectedState; }
            set { this.RaiseAndSetIfChanged(ref selectedState, value); }
        }

        public Uri WebUrl => null;

        public ReactiveCommand<object> OpenIssueOnGitHub { get; }

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            LocalRepository = repository;
            Load(false).Forget();
            return Task.CompletedTask;
        }

        public override Task Refresh() => Load(true);

        protected override void Dispose(bool disposing)
        {
            if (disposing && cancelLoad != null)
            {
                cancelLoad.Cancel();
                cancelLoad.Dispose();
                cancelLoad = null;
            }

            base.Dispose(disposing);
        }

        Task Load(bool refresh)
        {
            var sw = new Stopwatch();
            sw.Start();

            cancelLoad?.Cancel();
            cancelLoad?.Dispose();
            cancelLoad = new CancellationTokenSource();

            var issues = new VirtualizingList<IIssueListItemViewModel>(new IssueSource(this), null);
            Issues = issues;
            IssuesView = new VirtualizingListCollectionView<IIssueListItemViewModel>(issues);
            IssuesView.Filter = FilterItem;

            AuthorFilter = new UserFilterViewModel("Author", LoadAssignees);
            AssigneeFilter = new UserFilterViewModel("Assignee", LoadAssignees);

            return Task.CompletedTask;
        }

        Task<Page<ActorModel>> LoadAssignees(string after) => service.GetAssignees(LocalRepository, after);

        void DoOpenIssueOnGitHub(int number)
        {
            var repoUrl = LocalRepository.CloneUrl.ToRepositoryUrl();
            var url = repoUrl.Append("issues/" + number);
            visualStudioBrowser.OpenUrl(url);
        }

        bool FilterItem(object o)
        {
            var item = o as IIssueListItemViewModel;
            if (item == null) return false;

            if (!string.IsNullOrWhiteSpace(AuthorFilter?.Selected?.Login) &&
                item.Author?.Login != AuthorFilter.Selected?.Login)
            {
                return false;
            }

            switch (selectedState)
            {
                case IssueStateFilter.All:
                    return true;
                case IssueStateFilter.Open:
                    return item.State == IssueState.Open;
                case IssueStateFilter.Closed:
                    return item.State == IssueState.Closed;
            }

            return true;
        }

        static string CastFilter(string filter) => filter == NoFilter ? null : filter;

        class IssueSource : SequentialListSource<IssueListModel, IIssueListItemViewModel>
        {
            readonly IssueListViewModel owner;

            public IssueSource(IssueListViewModel owner)
            {
                this.owner = owner;
            }

            protected override IIssueListItemViewModel CreateViewModel(IssueListModel model)
            {
                var result = new IssueListItemViewModel(model);
                return result;
            }

            protected override Task<Page<IssueListModel>> LoadPage(string after)
            {
                return owner.service.GetIssues(
                    owner.LocalRepository,
                    after,
                    false);
            }

            protected override void OnBeginLoading()
            {
                owner.IsBusy = true;
            }

            protected override void OnEndLoading()
            {
                owner.IsBusy = false;
            }
        }
    }
}
