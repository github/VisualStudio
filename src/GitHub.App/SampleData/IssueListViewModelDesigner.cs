using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class IssueListViewModelDesigner : PanePageViewModelBase, IIssueListViewModel
    {
        public IssueListViewModelDesigner()
        {
            var issues = new[]
            {
                new IssueListModel
                {
                    Number = 1481,
                    Title = "When creating new pull request enable GitHub button",
                    Author = new ActorModel { Login = "meaghanlewis" },
                    UpdatedAt = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(1)),
                },
                new IssueListModel
                {
                    Number = 1482,
                    Title = "Add Issue Management to Visual Studio",
                    Author = new ActorModel { Login = "drobertson123" },
                    UpdatedAt = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(2)),
                },
                new IssueListModel
                {
                    Number = 1489,
                    Title = "Make Packages AsyncPackages",
                    Author = new ActorModel { Login = "grokys" },
                    UpdatedAt = DateTimeOffset.Now.Subtract(TimeSpan.FromHours(5)),
                },
            };

            Issues = new TrackingCollection<IIssueListItemViewModel>();
            foreach (var issue in issues) Issues.Add(new IssueListItemViewModel(issue));
        }

        public ITrackingCollection<IIssueListItemViewModel> Issues { get; }
        public string SearchQuery { get; set; }
        public Uri WebUrl { get; }
        public Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection) => Task.CompletedTask;
    }
}