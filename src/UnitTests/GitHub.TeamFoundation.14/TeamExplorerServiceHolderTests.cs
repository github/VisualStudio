using System;
using GitHub.Services;
using GitHub.VisualStudio.Base;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using NSubstitute;
using Xunit;

namespace UnitTests.GitHub.TeamFoundation._14
{
    public class TeamExplorerServiceHolderTests
    {
        [Fact]
        public void Foo()
        {
            var factory = Substitute.For<IUIContextFactory>();
            var uiContext = Substitute.For<IUIContextWrapper>();
            uiContext.IsActive.Returns(true);
            factory.FromUIContextGuid(Arg.Any<Guid>()).Returns(uiContext);

            var serviceProvider = Substitutes.ServiceProvider;
            var uiProvider = (IUIProvider)serviceProvider;
            var gitExt = Substitute.For<IGitExt>();
            var repositoryInfo = Substitute.For<IGitRepositoryInfo>();
            gitExt.ActiveRepositories.Returns(new[] { repositoryInfo });
            uiProvider.TryGetService(typeof(IGitExt)).Returns(gitExt);

            var target = new TeamExplorerServiceHolder(factory);

            target.ServiceProvider = serviceProvider;
        }
    }
}
