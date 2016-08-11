using System;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IPullRequestService
    {
        IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host, ISimpleRepositoryModel repository, string title, string body, IBranch source, IBranch target);
        string GetPullRequestTemplate(ISimpleRepositoryModel repository);
    }
}
