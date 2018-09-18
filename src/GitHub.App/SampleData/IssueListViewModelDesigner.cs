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
    public class IssueListViewModelDesigner : PanePageViewModelBase, IIssueListViewModel
    {
        public IssueListViewModelDesigner()
        {
            Items = new[]
            {
                new IssueListItemViewModelDesigner
                {
                    Number = 1855,
                    Title = "Do something sensible when things go wrong",
                    Author = new ActorViewModelDesigner("jcansdale"),
                    UpdatedAt = DateTimeOffset.Now - TimeSpan.FromDays(1),
                },
                new IssueListItemViewModelDesigner
                {
                    Number = 1865,
                    Title = "Installer hangs after download of GHfVS is complete",
                    Author = new ActorViewModelDesigner("meaghanlewis"),
                    CommentCount = 7,
                    UpdatedAt = DateTimeOffset.Now - TimeSpan.FromMinutes(2),
                },
                new IssueListItemViewModelDesigner
                {
                    Number = 1908,
                    Title = "Unicode error can occur when viewing diff for PR files",
                    Author = new ActorViewModelDesigner("StanleyGoldman"),
                    CommentCount = 0,
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
        public ILocalRepositoryModel LocalRepository { get; set; }
        public IssueListMessage Message { get; set; }
        public IRepositoryModel RemoteRepository { get; set; }
        public IReadOnlyList<IRepositoryModel> Forks { get; }
        public string SearchQuery { get; set; }
        public string SelectedState { get; set; }
        public string StateCaption { get; set; }
        public IReadOnlyList<string> States { get; }
        public Uri WebUrl => null;
        public ReactiveCommand<Unit> OpenItem { get; }
        public ReactiveCommand<object> OpenItemInBrowser { get; }

        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection) => Task.CompletedTask;
    }
}