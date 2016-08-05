using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Models;
using LibGit2Sharp;

namespace GitHub.Services
{
    [Export(typeof(IPullRequestService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestService : IPullRequestService
    {
        public IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host, ISimpleRepositoryModel repository, string title, string body, IBranch source, IBranch target)
        {
            return host.ModelService.CreatePullRequest(repository, title, body, source, target);
        }

        public void Checkout(ISimpleRepositoryModel repository, IPullRequestModel pullRequest, string localBranch)
        {
            var repo = GitService.GitServiceHelper.GetRepo(repository.LocalPath);
            var refspec = $"refs/pull/{pullRequest.Number}/head:refs/heads/{localBranch}";
            repo.Network.Fetch(repo.Network.Remotes["origin"], new[] { refspec }, new FetchOptions());
            repo.Checkout(localBranch);
        }
    }
}