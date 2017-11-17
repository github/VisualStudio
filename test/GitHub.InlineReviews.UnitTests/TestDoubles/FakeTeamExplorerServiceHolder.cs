using System;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.UnitTests.TestDoubles
{
    class FakeTeamExplorerServiceHolder : ITeamExplorerServiceHolder
    {
        ILocalRepositoryModel activeRepo;
        Action<ILocalRepositoryModel> listener;

        public FakeTeamExplorerServiceHolder()
        {
        }

        public FakeTeamExplorerServiceHolder(ILocalRepositoryModel repo)
        {
            ActiveRepo = repo;
        }

        public ILocalRepositoryModel ActiveRepo
        {
            get { return activeRepo; }
            set { activeRepo = value; NotifyActiveRepoChanged(); }
        }

        public IGitAwareItem HomeSection { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public void ClearServiceProvider(IServiceProvider provider)
        {
        }

        public void NotifyActiveRepoChanged()
        {
            listener?.Invoke(activeRepo);
        }

        public void Refresh()
        {
        }

        public void Subscribe(object who, Action<ILocalRepositoryModel> handler)
        {
            listener = handler;
            handler(ActiveRepo);
        }

        public void Unsubscribe(object who)
        {
        }
    }
}
