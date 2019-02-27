using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Collections;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using NSubstitute;
using NUnit.Framework;

namespace UnitTests.GitHub.App.ViewModels.GitHubPane
{
    public class IssueListViewModelBaseTests : TestBaseClass
    {
        [Test]
        public async Task First_State_Should_Be_Selected()
        {
            var target = await CreateTargetAndInitialize();

            Assert.That(target.SelectedState, Is.EqualTo("Open"));
        }

        [Test]
        public async Task Forks_Should_Be_Empty_If_No_Parent_Repository()
        {
            var target = await CreateTargetAndInitialize();

            Assert.That(target.Forks, Is.Null);
        }

        [Test]
        public async Task Forks_Should_Not_Be_Empty_If_Has_Parent_Repository()
        {
            var repositoryService = CreateRepositoryService("parent");
            var target = await CreateTargetAndInitialize(repositoryService: repositoryService);

            Assert.That(target.Forks, Is.Not.Null);
            Assert.That(target.Forks.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task Initializing_Loads_First_Page_Of_Items()
        {
            var target = await CreateTargetAndInitialize();

            await target.ItemSource.Received().GetPage(0);
        }

        [Test]
        public async Task With_Items_Returns_Message_None()
        {
            var target = await CreateTargetAndInitialize();

            Assert.That(target.Message, Is.EqualTo(IssueListMessage.None));
        }

        [Test]
        public async Task No_Items_No_Filter_Returns_Message_NoOpenItems()
        {
            var target = await CreateTargetAndInitialize(itemCount: 0);

            Assert.That(target.Message, Is.EqualTo(IssueListMessage.NoOpenItems));
        }

        [Test]
        public async Task No_Items_With_SearchQuery_Returns_Message_NoOpenItems()
        {
            var target = await CreateTargetAndInitialize(itemCount: 0);
            target.SearchQuery = "foo";

            Assert.That(target.Message, Is.EqualTo(IssueListMessage.NoItemsMatchCriteria));
        }

        [Test]
        public async Task No_Items_With_Closed_State_Returns_Message_NoOpenItems()
        {
            var target = await CreateTargetAndInitialize(itemCount: 0);
            target.SelectedState = "Closed";

            Assert.That(target.Message, Is.EqualTo(IssueListMessage.NoItemsMatchCriteria));
        }

        [Test]
        public async Task No_Items_With_Author_Filter_Returns_Message_NoOpenItems()
        {
            var target = await CreateTargetAndInitialize(itemCount: 0);
            target.AuthorFilter.Selected = target.AuthorFilter.Users[0];

            Assert.That(target.Message, Is.EqualTo(IssueListMessage.NoItemsMatchCriteria));
        }

        protected static LocalRepositoryModel CreateLocalRepository(
            string owner = "owner",
            string name = "name")
        {
            return new LocalRepositoryModel
            {
                CloneUrl = new UriString($"https://giuthub.com/{owner}/{name}"),
                Name = name
            };
        }

        protected static IPullRequestSessionManager CreateSessionManager(PullRequestDetailModel pullRequest = null)
        {
            pullRequest = pullRequest ?? new PullRequestDetailModel();

            var session = Substitute.For<IPullRequestSession>();
            session.PullRequest.Returns(pullRequest);

            var result = Substitute.For<IPullRequestSessionManager>();
            result.CurrentSession.Returns(session);
            return result;
        }

        protected static IPullRequestService CreatePullRequestService(int itemCount = 10)
        {
            var result = Substitute.For<IPullRequestService>();
            result.ReadPullRequests(null, null, null, null, null).ReturnsForAnyArgs(
                new Page<PullRequestListItemModel>
                {
                    Items = Enumerable.Range(0, itemCount).Select(x => new PullRequestListItemModel
                    {
                        Id = "pr" + x,
                        Number = x + 1,
                    }).ToList()
                });
            return result;
        }

        protected static IRepositoryService CreateRepositoryService(string parentOwnerLogin = null)
        {
            var result = Substitute.For<IRepositoryService>();
            var parent = parentOwnerLogin != null ? (parentOwnerLogin, "name") : ((string, string)?)null;
            result.FindParent(null, null, null).ReturnsForAnyArgs(parent);
            return result;
        }

        static Target CreateTarget(IRepositoryService repositoryService = null, int itemCount = 1000)
        {
            repositoryService = repositoryService ?? CreateRepositoryService();
            return new Target(repositoryService, itemCount);
        }

        static async Task<Target> CreateTargetAndInitialize(
            IRepositoryService repositoryService = null,
            LocalRepositoryModel repository = null,
            IConnection connection = null,
            int itemCount = 1000)
        {
            repository = repository ?? CreateLocalRepository();
            connection = connection ?? Substitute.For<IConnection>();

            var target = CreateTarget(repositoryService, itemCount);
            await target.InitializeAsync(repository, connection);
            return target;
        }

        class Target : IssueListViewModelBase
        {
            public Target(IRepositoryService repositoryService, int itemCount)
                : base(repositoryService)
            {
                ItemSource = Substitute.For<IVirtualizingListSource<IIssueListItemViewModelBase>>();
                ItemSource.GetCount().Returns(itemCount);
                ItemSource.PageSize.Returns(100);
            }

            public IVirtualizingListSource<IIssueListItemViewModelBase> ItemSource { get; }

            public override IReadOnlyList<string> States { get; } = new[] { "Open", "Closed" };

            protected override IVirtualizingListSource<IIssueListItemViewModelBase> CreateItemSource() => ItemSource;

            protected override Task DoOpenItem(IIssueListItemViewModelBase item)
            {
                throw new NotImplementedException();
            }

            protected override Task<Page<ActorModel>> LoadAuthors(string after)
            {
                return Task.FromResult(new Page<ActorModel>
                {
                    Items = new[]
                    {
                        new ActorModel { Login = "grokys" },
                        new ActorModel { Login = "jcansdale" },
                    },
                });
            }
        }
    }
}
