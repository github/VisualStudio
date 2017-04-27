using System;
using System.ComponentModel.Composition;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using NullGuard;

namespace GitHub.Services
{
    [Export(typeof(IPullRequestReviewSessionManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PullRequestReviewSessionManager : IPullRequestReviewSessionManager, IDisposable
    {
        readonly BehaviorSubject<IPullRequestReviewSession> sessionChanged = new BehaviorSubject<IPullRequestReviewSession>(null);

        public IObservable<IPullRequestReviewSession> SessionChanged => sessionChanged;

        public void NotifySessionChanged([AllowNull] IPullRequestReviewSession session)
        {
            sessionChanged.OnNext(session);
        }

        public void Dispose()
        {
            sessionChanged.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
