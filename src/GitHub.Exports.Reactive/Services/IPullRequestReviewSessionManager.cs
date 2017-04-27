using System;

namespace GitHub.Services
{
    public interface IPullRequestReviewSessionManager
    {
        IObservable<IPullRequestReviewSession> SessionChanged { get; }

        void NotifySessionChanged(IPullRequestReviewSession session);
    }
}
