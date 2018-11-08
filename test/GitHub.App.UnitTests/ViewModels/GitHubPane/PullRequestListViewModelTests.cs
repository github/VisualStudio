using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class PullRequestListViewModelTests : IssueListViewModelBaseTests
    {
        [Test]
        public async Task OpenItem_Navigates_To_Correct_Fork_Url()
        {
            var repository = CreateLocalRepository();
            var target = await CreateTargetAndInitialize(
                repositoryService: CreateRepositoryService("owner"),
                repository: CreateLocalRepository("fork", "name"));

            var uri = (Uri)null;
            target.NavigationRequested.Subscribe(x => uri = x);

            await target.OpenItem.Execute(target.Items[1]);

            Assert.That(uri, Is.EqualTo(new Uri("github://pane/owner/name/pull/2")));
        }

        static PullRequestListViewModel CreateTarget(
            IPullRequestSessionManager sessionManager = null,
            IRepositoryService repositoryService = null,
            IPullRequestService service = null)
        {
            sessionManager = sessionManager ?? CreateSessionManager();
            repositoryService = repositoryService ?? CreateRepositoryService();
            service = service ?? CreatePullRequestService();

            return new PullRequestListViewModel(
                sessionManager,
                repositoryService,
                service);
        }

        static async Task<PullRequestListViewModel> CreateTargetAndInitialize(
            IPullRequestSessionManager sessionManager = null,
            IRepositoryService repositoryService = null,
            IPullRequestService service = null,
            LocalRepositoryModel repository = null,
            IConnection connection = null)
        {
            var result = CreateTarget(sessionManager, repositoryService, service);
            await result.InitializeAsync(repository, connection);
            return result;
        }
    }
}
