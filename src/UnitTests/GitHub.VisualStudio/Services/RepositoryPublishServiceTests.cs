using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using Xunit;
using UnitTests;

public class RepositoryPublishServiceTests
{
    public class ThePublishMethod : TestBaseClass
    {
        [Fact]
        public async Task CreatesRepositoryAndPushesLocalToIt()
        {
            var solution = Substitute.For<IVsSolution>();
            var gitClient = Substitute.For<IGitClient>();
            var service = new RepositoryPublishService(gitClient, Substitutes.IVSGitServices);
            var newRepository = new Octokit.NewRepository("test");
            var account = Substitute.For<IAccount>();
            account.Login.Returns("monalisa");
            account.IsUser.Returns(true);
            var gitHubRepository = CreateRepository(account.Login, newRepository.Name);
            var apiClient = Substitute.For<IApiClient>();
            apiClient.CreateRepository(newRepository, "monalisa", true).Returns(Observable.Return(gitHubRepository));

            var repository = await service.PublishRepository(newRepository, account, apiClient);

            Assert.Equal("https://github.com/monalisa/test", repository.CloneUrl);
        }
    }
}
