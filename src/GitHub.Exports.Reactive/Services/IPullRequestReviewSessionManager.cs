using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IPullRequestReviewSessionManager
    {
        IObservable<IPullRequestReviewSession> CurrentSession { get; }

        Task<IPullRequestReviewSession> GetSession(IPullRequestModel pullRequest);
    }
}
