using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IPullRequestAnnotationsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestAnnotationsViewModel : PanePageViewModelBase, IPullRequestAnnotationsViewModel
    {
        public ILocalRepositoryModel LocalRepository { get; }
        public string RemoteRepositoryOwner { get; }
        public int PullRequestNumber { get; }
        public int CheckRunId { get; }

        public Task InitializeAsync(ILocalRepositoryModel localRepository, IConnection connection, string owner, string repo,
            int pullRequestNumber, int checkRunId)
        {
            return Task.CompletedTask;
        }
    }
}