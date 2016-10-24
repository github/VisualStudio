using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using NSubstitute;
using Octokit;
using Rothko;
using Xunit;

namespace UnitTests.GitHub.TeamFoundation._14
{
    public class TeamExplorerNavigationItemBaseTests
    {
        [Fact]
        public void ConstructorShouldSubscribeToTeamExplorerServiceHolder()
        {
            var apiFactory = Substitute.For<ISimpleApiClientFactory>();
            var holder = Substitute.For<ITeamExplorerServiceHolder>();
            var target = new TestNavigationItem(apiFactory, holder, Octicon.alert);

            holder.Received().Subscribe(target, Arg.Any<Action<ILocalRepositoryModel>>());
        }

        [Fact]
        public void RepoChangedShouldBeCalledWhenChangingToNewRepo()
        {
            var apiFactory = CreateApiClientFactory();
            var holder = Substitute.For<ITeamExplorerServiceHolder>();
            var os = new OperatingSystemFacade();
            Action<ILocalRepositoryModel> updateMethod = null;
            holder.Subscribe(Arg.Any<object>(), Arg.Do<Action<ILocalRepositoryModel>>(x => updateMethod = x));

            var target = new TestNavigationItem(apiFactory, holder, Octicon.alert);
            var repo1 = new LocalRepositoryModel(os, "repo1", new UriString("https://github.com/bar"), @"c:\foo\bar");
            var repo2 = new LocalRepositoryModel(os, "repo1", new UriString("https://github.com/baz"), @"c:\foo\bar");

            updateMethod(repo1);
            updateMethod(repo2);

            Assert.Equal(new[] { true, true }, target.RepoChangedNotifications.ToArray());
        }

        [Fact]
        public void RepoChangedShouldBeCalledWhenSameRepoIsSignalled()
        {
            var apiFactory = CreateApiClientFactory();
            var holder = Substitute.For<ITeamExplorerServiceHolder>();
            var os = new OperatingSystemFacade();
            Action<ILocalRepositoryModel> updateMethod = null;
            holder.Subscribe(Arg.Any<object>(), Arg.Do<Action<ILocalRepositoryModel>>(x => updateMethod = x));

            var target = new TestNavigationItem(apiFactory, holder, Octicon.alert);
            var repo1 = new LocalRepositoryModel(os, "repo1", new UriString("https://github.com/bar"), @"c:\foo\bar");
            var repo2 = new LocalRepositoryModel(os, "repo1", new UriString("https://github.com/bar"), @"c:\foo\bar");

            updateMethod(repo1);
            updateMethod(repo2);

            Assert.Equal(new[] { true, false }, target.RepoChangedNotifications.ToArray());
        }

        ISimpleApiClientFactory CreateApiClientFactory()
        {
            var apiFactory = Substitute.For<ISimpleApiClientFactory>();
            var apiClient = Substitute.For<ISimpleApiClient>();
            var repository = Substitute.For<Repository>();
            apiFactory.Create(Arg.Any<UriString>()).Returns(apiClient);
            apiClient.GetRepository().Returns(Task.FromResult(repository));
            return apiFactory;
        }

        class TestNavigationItem : TeamExplorerNavigationItemBase
        {
            public TestNavigationItem(ISimpleApiClientFactory apiFactory, ITeamExplorerServiceHolder holder, Octicon octicon)
                : base(apiFactory, holder, octicon)
            {
            }

            public IList<bool> RepoChangedNotifications { get; } = new List<bool>();

            protected override void RepoChanged(bool changed)
            {
                base.RepoChanged(changed);
                RepoChangedNotifications.Add(changed);
            }
        }
    }
}
