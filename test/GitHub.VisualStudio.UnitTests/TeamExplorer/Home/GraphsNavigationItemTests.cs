/*
 * Commented out to avoid:
 * 1>TeamExplorer\Home\GraphsNavigationItemTests.cs(24,44,24,64): 
 * error CS0433: The type 'GraphsNavigationItem' exists in both
 * 'GitHub.TeamFoundation.14, Version=2.5.9.0, Culture=neutral, PublicKeyToken=bc1bd09f2901c82e' and
 * 'GitHub.TeamFoundation.16, Version=2.5.9.0, Culture=neutral, PublicKeyToken=bc1bd09f2901c82e'

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
*/