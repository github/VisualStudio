using System;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Models;
using GitHub.Services;
using NSubstitute;
using Octokit;
using Xunit;

public class RepositoryCreationServiceTests
{
    public class TheCreateRepositoryMethod
    {
        [Fact]
        public void CreatesRepositoryOnlineViaApiAndThenClonesIt()
        {
            var newRepository = new NewRepository("octokit.net");
            var repository = new TestRepository("octokit.net", "https://github.com/octokit/octokit.net");
            var account = Substitute.For<IAccount>();
            account.Login.Returns("octokit");
            account.IsUser.Returns(false);
            var apiClient = Substitute.For<IApiClient>();
            apiClient.CreateRepository(newRepository, "octokit", false)
                .Returns(Observable.Return(repository));
            var cloneService = Substitute.For<IRepositoryCloneService>();
            cloneService.CloneRepository("https://github.com/octokit/octokit.net", "octokit.net", @"c:\dev")
                .Returns(Observable.Return(Unit.Default));
            var creator = new RepositoryCreationService(cloneService);

            creator.CreateRepository(newRepository, account, @"c:\dev", apiClient).Subscribe();

            apiClient.Received().CreateRepository(newRepository, "octokit", false);
        }

        public class TestRepository : Repository
        {
            public TestRepository(string name, string cloneUrl) : base()
            {
                Name = name;
                CloneUrl = cloneUrl;
            }
        }
    }
}
