using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Data;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestListViewModelDesigner : PanePageViewModelBase, IPullRequestListViewModel
    {
        public PullRequestListViewModelDesigner()
        {
            Items = new[]
            {
                new PullRequestListItemViewModelDesigner
                {
                    Number = 399,
                    IsCurrent = true,
                    Title = "Let's try doing this differently",
                    Author = new ActorViewModelDesigner("shana"),
                    UpdatedAt = DateTimeOffset.Now - TimeSpan.FromDays(1),
                },
                new PullRequestListItemViewModelDesigner
                {
                    Number = 389,
                    Title = "Build system upgrade",
                    Author = new ActorViewModelDesigner("haacked"),
                    CommentCount = 4,
                    UpdatedAt = DateTimeOffset.Now - TimeSpan.FromMinutes(2),
                },
                new PullRequestListItemViewModelDesigner
                {
                    Number = 409,
                    Title = "Fix publish button style and a really, really long name for this thing... OMG look how long this name is yusssss",
                    Author = new ActorViewModelDesigner("shana"),
                    CommentCount = 27,
                    UpdatedAt = DateTimeOffset.Now - TimeSpan.FromHours(5),
                },
            };

            ItemsView = CollectionViewSource.GetDefaultView(Items);
            States = new[] { "Open", "Closed", "All" };
            SelectedState = "Open";
        }

        public IUserFilterViewModel AuthorFilter { get; set; }
        public IReadOnlyList<IIssueListItemViewModelBase> Items { get; }
        public ICollectionView ItemsView { get; }
        public LocalRepositoryModel LocalRepository { get; set; }
        public IssueListMessage Message { get; set; }
        public RepositoryModel RemoteRepository { get; set; }
        public IReadOnlyList<RepositoryModel> Forks { get; }
        public string SearchQuery { get; set; }
        public string SelectedState { get; set; }
        public string StateCaption { get; set; }
        public IReadOnlyList<string> States { get; }
        public Uri WebUrl => null;
        public ReactiveCommand<Unit, Unit> CreatePullRequest { get; }
        public ReactiveCommand<IIssueListItemViewModelBase, Unit> OpenItem { get; }
        public ReactiveCommand<IPullRequestListItemViewModel, IPullRequestListItemViewModel> OpenItemInBrowser { get; }

        public Task InitializeAsync(LocalRepositoryModel repository, IConnection connection) => Task.CompletedTask;
    }
}