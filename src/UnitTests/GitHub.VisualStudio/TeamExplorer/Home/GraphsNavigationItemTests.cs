using System;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.TeamExplorer.Home;
using NSubstitute;
using Xunit;
using UnitTests;

public class GraphsNavigationItemTests
{
    public class TheExecuteMethod : TestBaseClass
    {
        [Theory(Skip = "Needs fixing with new TeamFoundation split assemblies")]
        [InlineData("https://github.com/foo/bar.git", "https://github.com/foo/bar/graphs")]
        [InlineData("https://haacked@github.com/foo/bar.git", "https://github.com/foo/bar/graphs")]
        [InlineData("https://github.com/foo/bar", "https://github.com/foo/bar/graphs")]
        [InlineData("https://github.com/foo/bar/", "https://github.com/foo/bar/graphs")]
        public void BrowsesToTheCorrectURL(string origin, string expectedUrl)
        {
            var apiFactory = Substitute.For<ISimpleApiClientFactory>();
            var browser = Substitute.For<IVisualStudioBrowser>();
            var lazyBrowser = new Lazy<IVisualStudioBrowser>(() => browser);
            var holder = Substitute.For<ITeamExplorerServiceHolder>();
            var graphsNavigationItem = new GraphsNavigationItem(Substitutes.ServiceProvider, apiFactory, lazyBrowser, holder)
            {
                ActiveRepoUri = origin
            };

            graphsNavigationItem.Execute();

            browser.Received().OpenUrl(new Uri(expectedUrl));
        }
    }
}
