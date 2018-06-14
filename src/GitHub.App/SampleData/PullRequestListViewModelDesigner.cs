using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Data;
using GitHub.Models;
using GitHub.ViewModels;
using GitHub.ViewModels.GitHubPane;

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
        }

        public IReadOnlyList<IViewModel> Items { get; }

        public ICollectionView ItemsView { get; }

        public ILocalRepositoryModel LocalRepository { get; set; }

        public string SearchQuery { get; set; }

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection) => Task.CompletedTask;
    }
}