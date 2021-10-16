using System;
using GitHub.Api;
using GitHub.Services;
using GitHub.VisualStudio.TeamExplorer.Home;
using NSubstitute;
using NUnit.Framework;
using UnitTests;

public class GraphsNavigationItemTests
{
    public class TheExecuteMethod : TestBaseClass
    {
        [TestCase("https://github.com/foo/bar.git", "https://github.com/foo/bar/graphs")]
        [TestCase("https://haacked@github.com/foo/bar.git", "https://github.com/foo/bar/graphs")]
        [TestCase("https://github.com/foo/bar", "https://github.com/foo/bar/graphs")]
        [TestCase("https://github.com/foo/bar/", "https://github.com/foo/bar/graphs")]
        [Ignore("Needs fixing with new TeamFoundation split assemblies")]
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
