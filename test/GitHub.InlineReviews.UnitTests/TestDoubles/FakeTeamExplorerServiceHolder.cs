using System;
using System.Collections.Generic;
using GitHub.Models;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using NSubstitute;
using UnitTests;

namespace GitHub.InlineReviews.UnitTests.TestDoubles
{
    class FakeVSGitExt : IVSGitExt
    {
        public IEnumerable<ILocalRepositoryModel> ActiveRepositories { get; set; }

        public event Action ActiveRepositoriesChanged;

        public void Refresh(IServiceProvider serviceProvider)
        {}

        public void SetActiveRepo(ILocalRepositoryModel activeRepo)
        {
            ActiveRepositories = new [] { activeRepo };
            ActiveRepositoriesChanged?.Invoke();
        }
    }

    class FakeTeamExplorerServiceHolder : TeamExplorerServiceHolder
    {
        static IVSGitExt SetupDefaultGitExt(ILocalRepositoryModel repo)
        {
            var gitExt = Substitute.For<IVSGitExt>();
            gitExt.ActiveRepositories.Returns(repo != null ? new[] { repo } : null);
            return gitExt;
        }

        public FakeTeamExplorerServiceHolder(ILocalRepositoryModel repo = null)
            : this(repo, SetupDefaultGitExt(repo))
        {}

        public FakeTeamExplorerServiceHolder(IVSGitExt gitExt)
            : this(null, gitExt, Substitute.For<IVSUIContextFactory>())
        {}

        public FakeTeamExplorerServiceHolder(ILocalRepositoryModel repo, IVSGitExt gitExt)
            : this(repo, gitExt, Substitute.For<IVSUIContextFactory>())
        {}

        FakeTeamExplorerServiceHolder(
            ILocalRepositoryModel repo,
            IVSGitExt gitExt,
            IVSUIContextFactory uiContextFactory)
            : base(uiContextFactory, gitExt)
        {
            var uiContext = Substitute.For<IVSUIContext>();
            uiContext.IsActive.Returns(true);
            uiContextFactory.GetUIContext(Arg.Any<Guid>()).Returns(uiContext);

            var serviceProvider = Substitutes.GetServiceProvider();
            serviceProvider.GetService(typeof(IVSGitExt)).Returns(gitExt);
            serviceProvider.GetService(typeof(IVSUIContextFactory)).Returns(uiContextFactory);
            ServiceProvider = serviceProvider;
        }
    }
}
