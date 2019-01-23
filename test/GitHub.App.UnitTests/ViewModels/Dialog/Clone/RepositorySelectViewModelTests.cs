using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels.Dialog.Clone;
using NSubstitute;
using NUnit.Framework;

public class RepositorySelectViewModelTests
{
    public class TheFilterProperty
    {
        [TestCase("unknown", "owner", "name", "https://github.com/owner/name", 0)]
        [TestCase("", "owner", "name", "https://github.com/owner/name", 1)]
        [TestCase("owner", "owner", "name", "https://github.com/owner/name", 1)]
        [TestCase("name", "owner", "name", "https://github.com/owner/name", 1)]
        [TestCase("owner/name", "owner", "name", "https://github.com/owner/name", 1)]
        [TestCase("OWNER/NAME", "owner", "name", "https://github.com/owner/name", 1)]
        public async Task Filter(string filter, string owner, string name, string url, int expectCount)
        {
            var contributedToRepositories = new[]
            {
                new RepositoryListItemModel
                {
                    Owner = owner,
                    Name = name,
                    Url = new Uri(url)
                }
            };
            var hostAddress = HostAddress.GitHubDotComHostAddress;
            var connection = CreateConnection(hostAddress);
            var repositoryCloneService = CreateRepositoryCloneService(contributedToRepositories, hostAddress);
            var target = new RepositorySelectViewModel(repositoryCloneService);
            target.Filter = filter;
            target.Initialize(connection);

            await target.Activate();

            var items = target.ItemsView.Groups
                .Cast<CollectionViewGroup>()
                .SelectMany(g => g.Items)
                .Cast<RepositoryItemViewModel>();
            Assert.That(items.Count, Is.EqualTo(expectCount));
        }

        static IConnection CreateConnection(HostAddress hostAddress)
        {
            var connection = Substitute.For<IConnection>();
            connection.HostAddress.Returns(hostAddress);
            return connection;
        }

        static IRepositoryCloneService CreateRepositoryCloneService(
            IList<RepositoryListItemModel> contributedToRepositories,
            HostAddress hostAddress)
        {
            var viewRepositoriesModel = CreateViewerRepositoriesModel(contributedToRepositories: contributedToRepositories);
            var repositoryCloneService = Substitute.For<IRepositoryCloneService>();
            repositoryCloneService.ReadViewerRepositories(hostAddress).Returns(viewRepositoriesModel);
            return repositoryCloneService;
        }

        private static ViewerRepositoriesModel CreateViewerRepositoriesModel(
            string owner = "owner",
            IList<RepositoryListItemModel> repositories = null,
            IList<RepositoryListItemModel> contributedToRepositories = null)
        {
            repositories = repositories ?? Array.Empty<RepositoryListItemModel>();
            contributedToRepositories = contributedToRepositories ?? Array.Empty<RepositoryListItemModel>();

            return new ViewerRepositoriesModel
            {
                Owner = owner,
                Repositories = CreateRepositoriesList(repositories),
                ContributedToRepositories = CreateRepositoriesList(contributedToRepositories),
                Organizations = CreateOrganizationsList()
            };
        }
    }

    static IReadOnlyList<RepositoryListItemModel> CreateRepositoriesList(IList<RepositoryListItemModel> repositories)
    {
        return repositories.ToList().AsReadOnly();
    }

    static IDictionary<string, IReadOnlyList<RepositoryListItemModel>> CreateOrganizationsList()
    {
        return new Dictionary<string, IReadOnlyList<RepositoryListItemModel>>();
    }
}
