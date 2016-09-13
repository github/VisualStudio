using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IPullRequestService
    {
        Task Checkout(ILocalRepositoryModel repository, IPullRequestModel pullRequest);

        IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host,
            ILocalRepositoryModel sourceRepository, IRepositoryModel targetRepository,
            IBranch sourceBranch, IBranch targetBranch,
            string title, string body);

        IObservable<string> GetPullRequestTemplate(ILocalRepositoryModel repository);
    }
}
