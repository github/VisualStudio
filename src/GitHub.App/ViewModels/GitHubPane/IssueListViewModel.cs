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

        readonly IIssueService service;
        CancellationTokenSource cancelLoad;
        ObservableCollection<string> assignees;
        ObservableCollection<string> authors;
        string searchQuery;
        string selectedAssignee;
        string selectedAuthor;
        IssueStateFilter selectedState = IssueStateFilter.Open;

        [ImportingConstructor]
        public IssueListViewModel(IIssueService service)
        {
            this.service = service;
            assignees = new ObservableCollection<string>(NoFilterList);
            authors = new ObservableCollection<string>(NoFilterList);
            Issues = new TrackingCollection<IIssueListItemViewModel>();
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
        }

        public IReadOnlyList<string> Assignees => assignees;
        public IReadOnlyList<string> Authors => authors;
        public IReadOnlyList<IssueStateFilter> States { get; }
        public ITrackingCollection<IIssueListItemViewModel> Issues { get; }
        public ILocalRepositoryModel LocalRepository { get; private set; }

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

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            LocalRepository = repository;
            Load(false).Forget();
            return Task.CompletedTask;
        }

        public override Task Refresh() => Load(true);

        Task Load(bool refresh)
        {
            IsBusy = true;

            var sw = new Stopwatch();
            sw.Start();

            cancelLoad?.Cancel();
            cancelLoad?.Dispose();
            cancelLoad = new CancellationTokenSource();

            var items = service.GetIssues(LocalRepository, cancelLoad.Token, refresh)
                .SelectMany(page => page.Items.Select(x => new IssueListItemViewModel(x)));

            Issues.Clear();
            Issues.Listen(items);

            Issues.OriginalCompleted.Subscribe(_ =>
            {
                sw.Stop();
                Debug.WriteLine("Loaded issues in " + sw.Elapsed);
                IsBusy = false;
                cancelLoad.Dispose();
                cancelLoad = null;
            });
            
            Issues.Subscribe(x =>
            {
                if (x.Author != null && !authors.Contains(x.Author.Login)) authors.Add(x.Author.Login);

                foreach (var assignee in x.Assignees)
                {
                    if (!assignees.Contains(assignee.Login))
                    {
                        assignees.Add(assignee.Login);
                    }
                }
            }, () => { });

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

            if (!Issues.Disposed)
            {
                Issues.Filter = (issue, index, list) =>
                    (state == IssueStateFilter.All || (int)issue.State == (int)state) &&
                    (assignee == null || issue.Assignees.Any(x => x.Login == assignee)) &&
                    (author == null || author == issue.Author?.Login) &&
                    (filterTextIsNumber == false || issue.Number == filterPullRequestNumber) &&
                    (filterTextIsString == false || issue.Title.ToUpperInvariant().Contains(filter.ToUpperInvariant()));
            }
        }

        static string CastFilter(string filter)
        {
            return filter == NoFilter ? null : filter;
        }
    }
}
