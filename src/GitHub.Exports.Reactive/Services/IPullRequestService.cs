using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IPullRequestService
    {
        Task Checkout(ISimpleRepositoryModel repository, IPullRequestModel pullRequest);

        IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host,
            ISimpleRepositoryModel sourceRepository, ISimpleRepositoryModel targetRepository,
            IBranch sourceBranch, IBranch targetBranch,
            string title, string body);

        IObservable<string> GetPullRequestTemplate(ISimpleRepositoryModel repository);
    }
}
