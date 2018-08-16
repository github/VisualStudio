using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public sealed class PullRequestAnnotationsViewModelDesigner : PanePageViewModelBase, IPullRequestAnnotationsViewModel
    {
        public ILocalRepositoryModel LocalRepository { get; set; }
        public string RemoteRepositoryOwner { get; set; }
        public int PullRequestNumber { get; set; }
        public int CheckRunId { get; set; }

        public Task InitializeAsync(ILocalRepositoryModel localRepository, IConnection connection, string owner, string repo,
            int pullRequestNumber, int checkRunId)
        {
            return Task.CompletedTask;
        }
    }
}