using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IPullRequestSessionManager
    {
        IObservable<IPullRequestSession> CurrentSession { get; }

        Task<IPullRequestSession> GetSession(IPullRequestModel pullRequest);
    }
}
