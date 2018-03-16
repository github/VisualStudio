using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestListViewModelDesigner : PanePageViewModelBase, IPullRequestListViewModel
    {
        public PullRequestListViewModelDesigner()
        {
            var prs = new TrackingCollection<IPullRequestModel>(Observable.Empty<IPullRequestModel>());
            prs.Add(new PullRequestModel(399, "Let's try doing this differently",
                new AccountDesigner { Login = "shana", IsUser = true },
                DateTimeOffset.Now - TimeSpan.FromDays(1))
            {
                Assignee = new AccountDesigner { Login = "shana", IsUser = true },
            });
            prs.Add(new PullRequestModel(389, "Build system upgrade",
                new AccountDesigner { Login = "shana", IsUser = true },
                DateTimeOffset.Now - TimeSpan.FromMinutes(2))
            {
                CommentCount = 4,
                HasNewComments = false,
                Assignee = new AccountDesigner { Login = "haacked", IsUser = true },
            });
            prs.Add(new PullRequestModel(409, "Fix publish button style and a really, really long name for this thing... OMG look how long this name is yusssss",
                new AccountDesigner { Login = "shana", IsUser = true },
                DateTimeOffset.Now - TimeSpan.FromHours(5))
            {
                CommentCount = 27,
                HasNewComments = true,
                Assignee = new AccountDesigner { Login = "Haacked", IsUser = true },
            });
            PullRequests = prs;

            States = new List<PullRequestState> {
                new PullRequestState { IsOpen = true, Name = "Open" },
                new PullRequestState { IsOpen = false, Name = "Closed" },
                new PullRequestState { Name = "All" }
            };
            SelectedState = States[0];
            Assignees = new ObservableCollection<IAccount>(prs.Select(x => x.Assignee));
            Authors = new ObservableCollection<IAccount>(prs.Select(x => x.Author));
            SelectedAssignee = Assignees.ElementAt(1);
            SelectedAuthor = Authors.ElementAt(1);

            IsLoaded = true;
        }

        public IReadOnlyList<IRemoteRepositoryModel> Repositories { get; }
        public IRemoteRepositoryModel SelectedRepository { get; set; }
        public IPullRequestModel CheckedOutPullRequest { get; set; }
        public ITrackingCollection<IPullRequestModel> PullRequests { get; set; }
        public IPullRequestModel SelectedPullRequest { get; set; }

        public IReadOnlyList<PullRequestState> States { get; set; }
        public PullRequestState SelectedState { get; set; }

        public ObservableCollection<IAccount> Authors { get; set; }
        public IAccount SelectedAuthor { get; set; }
        public bool RepositoryIsFork { get; set; } = true;
        public bool ShowPullRequestsForFork { get; set; }
        public string SearchQuery { get; set; }
        public bool IsLoaded { get; }

        public ObservableCollection<IAccount> Assignees { get; set; }
        public IAccount SelectedAssignee { get; set; }
        public Uri WebUrl { get; set; }

        public ReactiveCommand<object> OpenPullRequest { get; }
        public ReactiveCommand<object> CreatePullRequest { get; }
        public ReactiveCommand<object> OpenPullRequestOnGitHub { get; }

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection) => Task.CompletedTask;
    }
}