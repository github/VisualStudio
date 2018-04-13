using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using GitHub.Collections;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using GitHub.ViewModels.GitHubPane;
using GitHub.Primitives;
using NSubstitute;
using NUnit.Framework;
using GitHub.Factories;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestListViewModelTests : TestBaseClass
    {
        [Test]
        public void SelectingAssigneeShouldTriggerFilter()
        {
            var serviceProvider = Substitutes.ServiceProvider;
            var connection = serviceProvider.GetConnection();
            var factory = CreateModelServiceFactory();
            var repository = Substitute.For<ILocalRepositoryModel>();
            var settings = serviceProvider.TryGetServiceSync<IPackageSettings>();
            settings.UIState.Returns(new UIState());
            var sessionManager = Substitute.For<IPullRequestSessionManager>();
            var browser = Substitute.For<IVisualStudioBrowser>();
            var prViewModel = new PullRequestListViewModel(serviceProvider, factory, sessionManager, browser);

            prViewModel.InitializeAsync(repository, connection).Wait();
            prViewModel.PullRequests.Received(1).Filter = AnyFilter;

            prViewModel.SelectedAssignee = prViewModel.PullRequests[0].Assignee;
            prViewModel.PullRequests.Received(2).Filter = AnyFilter;
        }

        [Test]
        public void ResettingAssigneeToNoneShouldNotTriggerFilter()
        {
            var serviceProvider = Substitutes.ServiceProvider;
            var connection = serviceProvider.GetConnection();
            var factory = CreateModelServiceFactory();
            var repository = Substitute.For<ILocalRepositoryModel>();
            var settings = serviceProvider.TryGetServiceSync<IPackageSettings>();
            settings.UIState.Returns(new UIState());
            var sessionManager = Substitute.For<IPullRequestSessionManager>();
            var browser = Substitute.For<IVisualStudioBrowser>();
            var prViewModel = new PullRequestListViewModel(serviceProvider, factory, sessionManager, browser);

            prViewModel.InitializeAsync(repository, connection).Wait();
            prViewModel.PullRequests.Received(1).Filter = AnyFilter;

            prViewModel.SelectedAssignee = prViewModel.PullRequests[0].Assignee;
            prViewModel.PullRequests.Received(2).Filter = AnyFilter;

            // Setting the Assignee filter to [None] should not trigger a filter:
            // doing this will remove the [None] entry from Assignees, which will cause
            // the selection in the view to be set to null which will reset the filter.
            prViewModel.SelectedAssignee = prViewModel.EmptyUser;
            prViewModel.PullRequests.Received(2).Filter = AnyFilter;
        }

        [Test]
        public void SelectingAuthorShouldTriggerFilter()
        {
            var serviceProvider = Substitutes.ServiceProvider;
            var connection = serviceProvider.GetConnection();
            var factory = CreateModelServiceFactory();
            var repository = Substitute.For<ILocalRepositoryModel>();
            var settings = serviceProvider.TryGetServiceSync<IPackageSettings>();
            settings.UIState.Returns(new UIState());
            var sessionManager = Substitute.For<IPullRequestSessionManager>();
            var browser = Substitute.For<IVisualStudioBrowser>();
            var prViewModel = new PullRequestListViewModel(serviceProvider, factory, sessionManager, browser);

            prViewModel.InitializeAsync(repository, connection).Wait();
            prViewModel.PullRequests.Received(1).Filter = AnyFilter;

            prViewModel.SelectedAuthor = prViewModel.PullRequests[0].Author;
            prViewModel.PullRequests.Received(2).Filter = AnyFilter;
        }

        [Test]
        public void ResettingAuthorToNoneShouldNotTriggerFilter()
        {
            var serviceProvider = Substitutes.ServiceProvider;
            var connection = serviceProvider.GetConnection();
            var factory = CreateModelServiceFactory();
            var repository = Substitute.For<ILocalRepositoryModel>();
            var settings = serviceProvider.TryGetServiceSync<IPackageSettings>();
            settings.UIState.Returns(new UIState());
            var sessionManager = Substitute.For<IPullRequestSessionManager>();
            var browser = Substitute.For<IVisualStudioBrowser>();
            var prViewModel = new PullRequestListViewModel(serviceProvider, factory, sessionManager, browser);

            prViewModel.InitializeAsync(repository, connection).Wait();
            prViewModel.PullRequests.Received(1).Filter = AnyFilter;

            prViewModel.SelectedAuthor = prViewModel.PullRequests[0].Author;
            prViewModel.PullRequests.Received(2).Filter = AnyFilter;

            // Setting the Author filter to [None] should not trigger a filter:
            // doing this will remove the [None] entry from Authors, which will cause
            // the selection in the view to be set to null which will reset the filter.
            prViewModel.SelectedAuthor = prViewModel.EmptyUser;
            prViewModel.PullRequests.Received(2).Filter = AnyFilter;
        }

        [TestCase("https://github.com/owner/repo", 666, "https://github.com/owner/repo/pull/666")]
        public void OpenPullRequestOnGitHubShouldOpenBrowser(string cloneUrl, int pullNumber, string expectUrl)
        {
            var serviceProvider = Substitutes.ServiceProvider;
            var connection = serviceProvider.GetConnection();
            var factory = CreateModelServiceFactory();
            var repository = Substitute.For<ILocalRepositoryModel>();
            var settings = serviceProvider.TryGetServiceSync<IPackageSettings>();
            settings.UIState.Returns(new UIState());
            var sessionManager = Substitute.For<IPullRequestSessionManager>();
            var browser = Substitute.For<IVisualStudioBrowser>();
            var prViewModel = new PullRequestListViewModel(serviceProvider, factory, sessionManager, browser);

            prViewModel.InitializeAsync(repository, connection).Wait();
            prViewModel.SelectedRepository = Substitute.For<IRemoteRepositoryModel>();
            prViewModel.SelectedRepository.CloneUrl.Returns(new UriString(cloneUrl));

            prViewModel.OpenPullRequestOnGitHub.Execute(pullNumber);

            browser.ReceivedWithAnyArgs(1).OpenUrl(new Uri(expectUrl));
        }

        Func<IPullRequestModel, int, IList<IPullRequestModel>, bool> AnyFilter =>
            Arg.Any<Func<IPullRequestModel, int, IList<IPullRequestModel>, bool>>();

        IModelServiceFactory CreateModelServiceFactory()
        {
            var modelService = Substitute.For<IModelService>();
            var bitmapSource = Observable.Empty<BitmapImage>();

            var pullRequest = new PullRequestModel(
                1,
                "PR1",
                new Account("foo", true, false, 1, 0, bitmapSource),
                DateTimeOffset.MinValue);
            pullRequest.Assignee = new Account("foo", true, false, 1, 0, bitmapSource);

            var pullRequestCollection = Substitute.For<ITrackingCollection<IPullRequestModel>>();
            pullRequestCollection[0].Returns(pullRequest);

            modelService.GetPullRequests(
                Arg.Any<ILocalRepositoryModel>(),
                Arg.Any<ITrackingCollection<IPullRequestModel>>())
               .Returns(pullRequestCollection);

            var result = Substitute.For<IModelServiceFactory>();
            result.CreateAsync(null).ReturnsForAnyArgs(modelService);
            result.CreateBlocking(null).ReturnsForAnyArgs(modelService);
            return result;
        }
    }
}
