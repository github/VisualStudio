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
        IPullRequestReviewSession current;

        public IObservable<IPullRequestReviewSession> SessionChanged => sessionChanged;

        public void NotifySessionChanged([AllowNull] IPullRequestReviewSession session)
        {
            current = session;
            sessionChanged.OnNext(session);
        }

        public IDisposable OpeningCompareViewHack(string path)
        {
            if (current != null)
            {
                return current.OpeningCompareViewHack(path);
            }

            return Disposable.Empty;
        }

        public void Dispose()
        {
            sessionChanged.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
