using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using Xunit;

public class RepositoryPublishServiceTests
{
    public class ThePublishMethod
    {
        [Fact]
        public async Task CreatesRepositoryAndPushesLocalToIt()
        {
            var solution = Substitute.For<IVsSolution>();
            var gitClient = Substitute.For<IGitClient>();
            var service = new RepositoryPublishService(gitClient, Substitute.For<IVSServices>());
            var newRepository = new Octokit.NewRepository("test");
            var account = Substitute.For<IAccount>();
            account.Login.Returns("monalisa");
            account.IsUser.Returns(true);
            var gitHubRepository = CreateOctokitRepository("https://github.com/monalisa/test");
            var apiClient = Substitute.For<IApiClient>();
            apiClient.CreateRepository(newRepository, "monalisa", true).Returns(Observable.Return(gitHubRepository));

            var repository = await service.PublishRepository(newRepository, account, apiClient);

            Assert.Equal("https://github.com/monalisa/test", repository.CloneUrl);
        }

        static Octokit.Repository CreateOctokitRepository(string cloneUrl)
        {
            var notCloneUrl = cloneUrl + "x";
            return new Octokit.Repository(notCloneUrl, notCloneUrl, cloneUrl, notCloneUrl, notCloneUrl, notCloneUrl, notCloneUrl, 1, null, null, null, null, null, null, false, false, 0, 0, 0, "master", 0, null, DateTimeOffset.Now, DateTimeOffset.Now, null, null, null, null, false, false, false);
        }
    }
}
