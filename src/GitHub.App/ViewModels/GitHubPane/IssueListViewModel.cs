using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
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
        ObservableCollection<string> assignees;
        ObservableCollection<string> authors;
        string searchQuery;
        string selectedAssignee;
        string selectedAuthor;
        IssueStateFilter selectedState = IssueStateFilter.Open;

        [ImportingConstructor]
        public IssueListViewModel(
            IIssueService service,
            IVisualStudioBrowser visualStudioBrowser)
        {
            this.service = service;
            this.visualStudioBrowser = visualStudioBrowser;

            assignees = new ObservableCollection<string>(NoFilterList);
            authors = new ObservableCollection<string>(NoFilterList);
            States = new[] { IssueStateFilter.Open, IssueStateFilter.Closed, IssueStateFilter.All };
            Title = Resources.IssuesNavigationItemText;

            this.WhenAnyValue(x => x.SelectedState)
                .Subscribe(s => UpdateFilter(s, SelectedAssignee, SelectedAuthor, SearchQuery));
            this.WhenAnyValue(x => x.SelectedAssignee)
                .Subscribe(a => UpdateFilter(SelectedState, a, SelectedAuthor, SearchQuery));
            this.WhenAnyValue(x => x.SelectedAuthor)
                .Subscribe(a => UpdateFilter(SelectedState, SelectedAssignee, a, SearchQuery));
            this.WhenAnyValue(x => x.SearchQuery)
                .Subscribe(f => UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor, f));

            OpenIssueOnGitHub = ReactiveCommand.Create()
                .OnExecuteCompleted(x => DoOpenIssueOnGitHub((int)x));
        }

        public IReadOnlyList<string> Assignees => assignees;
        public IReadOnlyList<string> Authors => authors;
        public IReadOnlyList<IssueStateFilter> States { get; }
        public ILocalRepositoryModel LocalRepository { get; private set; }

        public IReadOnlyList<IIssueListItemViewModel> Issues
        {
            get { return issues; }
            private set { this.RaiseAndSetIfChanged(ref issues, value); }
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

        public string SelectedAuthor
        {
            get { return selectedAuthor; }
            set { this.RaiseAndSetIfChanged(ref selectedAuthor, CastFilter(value)); }
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

        Task Load(bool refresh)
        {
            var sw = new Stopwatch();
            sw.Start();

            cancelLoad?.Cancel();
            cancelLoad?.Dispose();
            cancelLoad = new CancellationTokenSource();

            Issues = new VirtualizingList<IIssueListItemViewModel>(new IssueSource(this), null);

            return Task.CompletedTask;
        }

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

        void DoOpenIssueOnGitHub(int number)
        {
            var repoUrl = LocalRepository.CloneUrl.ToRepositoryUrl();
            var url = repoUrl.Append("issues/" + number);
            visualStudioBrowser.OpenUrl(url);
        }

        void UpdateFilter(IssueStateFilter state, string assignee, string author, string filter)
        {
            var filterTextIsNumber = false;
            var filterTextIsString = false;
            var filterPullRequestNumber = 0;

            if (filter != null)
            {
                filter = filter.Trim();

                var hasText = !string.IsNullOrEmpty(filter);

                if (hasText && filter.StartsWith("#", StringComparison.CurrentCultureIgnoreCase))
                {
                    filterTextIsNumber = int.TryParse(filter.Substring(1), out filterPullRequestNumber);
                }
                else
                {
                    filterTextIsNumber = int.TryParse(filter, out filterPullRequestNumber);
                }

                filterTextIsString = hasText && !filterTextIsNumber;
            }
        }

        static string CastFilter(string filter)
        {
            return filter == NoFilter ? null : filter;
        }

        class IssueSource : SequentialListSource<IssueListModel, IIssueListItemViewModel>
        {
            readonly IssueListViewModel owner;

            public IssueSource(IssueListViewModel owner)
            {
                this.owner = owner;
            }

            protected override IIssueListItemViewModel CreateViewModel(IssueListModel model)
            {
                return new IssueListItemViewModel(model);
            }

            protected override Task<Page<IssueListModel>> LoadPage(string after)
            {
                return owner.service.GetIssues(
                    owner.LocalRepository,
                    after,
                    false);
            }

            protected override void OnBeginLoading(int nextPage, int toPage)
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
