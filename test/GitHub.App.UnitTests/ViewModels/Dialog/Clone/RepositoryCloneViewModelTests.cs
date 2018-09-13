using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.ViewModels.Dialog.Clone;
using NSubstitute;
using NUnit.Framework;
using Rothko;

namespace GitHub.App.UnitTests.ViewModels.Dialog.Clone
{
    public class RepositoryCloneViewModelTests
    {
        [Test]
        public async Task GitHubPage_Is_Initialized()
        {
            var cm = CreateConnectionManager("https://github.com");
            var target = CreateTarget(connectionManager: cm);

            await target.InitializeAsync(null);

            target.GitHubTab.Received(1).Initialize(cm.Connections[0]);
            target.EnterpriseTab.DidNotReceiveWithAnyArgs().Initialize(null);
        }

        [Test]
        public async Task EnterprisePage_Is_Initialized()
        {
            var cm = CreateConnectionManager("https://enterprise.com");
            var target = CreateTarget(connectionManager: cm);

            await target.InitializeAsync(null);

            target.GitHubTab.DidNotReceiveWithAnyArgs().Initialize(null);
            target.EnterpriseTab.Received(1).Initialize(cm.Connections[0]);
        }

        [Test]
        public async Task GitHub_And_Enterprise_Pages_Are_Initialized()
        {
            var cm = CreateConnectionManager("https://github.com", "https://enterprise.com");
            var target = CreateTarget(connectionManager: cm);

            await target.InitializeAsync(null);

            target.GitHubTab.Received(1).Initialize(cm.Connections[0]);
            target.EnterpriseTab.Received(1).Initialize(cm.Connections[1]);
        }

        [Test]
        public async Task GitHubPage_Is_Loaded()
        {
            var cm = CreateConnectionManager("https://github.com", "https://enterprise.com");
            var target = CreateTarget(connectionManager: cm);

            await target.InitializeAsync(null);

            await target.GitHubTab.Received(1).Activate();
            await target.EnterpriseTab.DidNotReceive().Activate();
        }

        [Test]
        public async Task Enterprise_Is_Loaded()
        {
            var cm = CreateConnectionManager("https://github.com", "https://enterprise.com");
            var target = CreateTarget(connectionManager: cm);

            await target.InitializeAsync(cm.Connections[1]);

            await target.GitHubTab.DidNotReceive().Activate();
            await target.EnterpriseTab.Received(1).Activate();
        }

        [Test]
        public async Task Switching_To_GitHubPage_Loads_It()
        {
            var cm = CreateConnectionManager("https://github.com", "https://enterprise.com");
            var target = CreateTarget(connectionManager: cm);

            await target.InitializeAsync(cm.Connections[1]);
            await target.GitHubTab.DidNotReceive().Activate();

            target.SelectedTabIndex = 0;

            await target.GitHubTab.Received(1).Activate();
        }

        [Test]
        public async Task Switching_To_EnterprisePage_Loads_It()
        {
            var cm = CreateConnectionManager("https://github.com", "https://enterprise.com");
            var target = CreateTarget(connectionManager: cm);

            await target.InitializeAsync(cm.Connections[0]);
            await target.EnterpriseTab.DidNotReceive().Activate();

            target.SelectedTabIndex = 1;

            await target.EnterpriseTab.Received(1).Activate();
        }

        [Test]
        public async Task Path_Is_Initialized()
        {
            var target = CreateTarget();

            Assert.That(target.Path, Is.EqualTo("d:\\efault\\path"));
        }

        [Test]
        public async Task Owner_And_Repository_Name_Is_Appended_To_Base_Path()
        {
            var target = CreateTarget();

            SetRepository(target.GitHubTab, CreateRepositoryModel("owner", "repo"));

            Assert.That(target.Path, Is.EqualTo("d:\\efault\\path\\owner\\repo"));
        }

        [Test]
        public async Task PathError_Is_Not_Set_When_No_Repository_Selected()
        {
            var target = CreateTarget();

            target.Path = "d:\\exists";

            Assert.That(target.PathError, Is.Null);
        }

        [Test]
        public async Task PathError_Is_Set_For_Existing_Destination()
        {
            var target = CreateTarget();
            SetRepository(target.GitHubTab, CreateRepositoryModel("owner", "repo"));
            target.Path = "d:\\exists";

            Assert.That(target.PathError, Is.EqualTo(Resources.DestinationAlreadyExists));
        }

        [Test]
        public async Task Repository_Name_Replaces_Last_Part_Of_Non_Base_Path()
        {
            var target = CreateTarget();

            var owner = "owner";
            target.Path = "d:\\efault";
            SetRepository(target.GitHubTab, CreateRepositoryModel(owner, "name"));
            target.Path = $"d:\\efault\\{owner}\\foo";
            SetRepository(target.GitHubTab, CreateRepositoryModel(owner, "repo"));

            Assert.That(target.Path, Is.EqualTo($"d:\\efault\\{owner}\\repo"));
        }

        [TestCase("c:\\base", "owner1/repo1", "c:\\base\\owner1\\repo1", "owner2/repo2", "c:\\base\\owner2\\repo2",
            Description = "Path unchanged")]
        [TestCase("c:\\base", "owner1/repo1", "c:\\base\\owner1\\changed", "owner2/repo2", "c:\\base\\owner2\\repo2",
            Description = "Repo name changed")]
        [TestCase("c:\\base", "owner1/repo1", "c:\\base\\owner1", "owner2/repo2", "c:\\base\\owner2\\repo2",
            Description = "Repo name deleted")]
        [TestCase("c:\\base", "owner1/repo1", "c:\\base", "owner2/repo2", "c:\\base\\owner2\\repo2",
            Description = "Base path reverted")]

        [TestCase("c:\\base", "owner1/repo1", "c:\\new\\base\\owner1\\changed", "owner2/repo2", "c:\\new\\base\\owner2\\repo2",
            Description = "Base path and repo name changed")]
        [TestCase("c:\\base", "owner1/repo1", "c:\\new\\base\\owner1", "owner2/repo2", "c:\\new\\base\\owner2\\repo2",
            Description = "Base path changed and repo name deleted")]
        [TestCase("c:\\base", "owner1/repo1", "c:\\new\\base", "owner2/repo2", "c:\\new\\base\\owner2\\repo2",
            Description = "Base path changed and repo owner/name deleted")]

        [TestCase("c:\\base", "owner1/repo1", "", "owner2/repo2", "c:\\base\\owner2\\repo2",
            Description = "Base path cleared")]
        [TestCase("c:\\base", "owner1/repo1", "c:\\base\\repo1", "owner2/repo2", "c:\\base\\owner2\\repo2",
            Description = "Owner deleted")]
        [TestCase("c:\\base", "same/same", "c:\\base\\same\\same", "owner2/repo2", "c:\\base\\owner2\\repo2",
            Description = "Owner and repo have same name")]
        public async Task User_Edits_Path(string defaultClonePath, string repo1, string userPath, string repo2, string expectPath)
        {
            var target = CreateTarget(defaultClonePath: defaultClonePath);
            SetRepository(target.GitHubTab, CreateRepositoryModel(repo1));
            target.Path = userPath;

            SetRepository(target.GitHubTab, CreateRepositoryModel(repo2));

            Assert.That(target.Path, Is.EqualTo(expectPath));
        }

        [Test]
        public async Task Clone_Is_Initially_Disabled()
        {
            var target = CreateTarget();

            await target.InitializeAsync(null);

            Assert.That(target.Clone.CanExecute(null), Is.False);
        }

        [Test]
        public async Task Clone_Is_Enabled_When_Repository_Selected()
        {
            var target = CreateTarget();

            await target.InitializeAsync(null);

            SetRepository(target.GitHubTab, CreateRepositoryModel());

            Assert.That(target.Clone.CanExecute(null), Is.True);
        }

        [Test]
        public async Task Clone_Is_Disabled_When_Has_PathError()
        {
            var target = CreateTarget();

            await target.InitializeAsync(null);

            SetRepository(target.GitHubTab, CreateRepositoryModel());
            Assert.That(target.Clone.CanExecute(null), Is.True);

            target.Path = "d:\\exists";

            Assert.That(target.Clone.CanExecute(null), Is.False);
        }

        static void SetRepository(IRepositoryCloneTabViewModel vm, IRepositoryModel repository)
        {
            vm.Repository.Returns(repository);
            vm.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(
                vm,
                new PropertyChangedEventArgs(nameof(vm.Repository)));
        }

        static IConnectionManager CreateConnectionManager(params string[] addresses)
        {
            var result = Substitute.For<IConnectionManager>();
            var connections = new ObservableCollectionEx<IConnection>();

            result.Connections.Returns(connections);
            result.GetLoadedConnections().Returns(connections);
            result.GetConnection(null).ReturnsForAnyArgs(default(IConnection));

            foreach (var address in addresses)
            {
                var connection = Substitute.For<IConnection>();
                var hostAddress = HostAddress.Create(address);
                connection.HostAddress.Returns(hostAddress);
                connection.IsLoggedIn.Returns(true);
                connections.Add(connection);
                result.GetConnection(hostAddress).Returns(connection);
            }

            return result;
        }

        static IRepositorySelectViewModel CreateSelectViewModel()
        {
            var result = Substitute.For<IRepositorySelectViewModel>();
            result.Repository.Returns((IRepositoryModel)null);
            result.WhenForAnyArgs(x => x.Initialize(null)).Do(_ => result.IsEnabled.Returns(true));
            return result;
        }

        static IRepositoryCloneService CreateRepositoryCloneService(string defaultClonePath)
        {
            var result = Substitute.For<IRepositoryCloneService>();
            result.DefaultClonePath.Returns(defaultClonePath);
            result.DestinationExists("d:\\exists").Returns(true);
            return result;
        }

        static RepositoryCloneViewModel CreateTarget(
            IOperatingSystem os = null,
            IConnectionManager connectionManager = null,
            IRepositoryCloneService service = null,
            IRepositorySelectViewModel gitHubTab = null,
            IRepositorySelectViewModel enterpriseTab = null,
            IRepositoryUrlViewModel urlTab = null,
            string defaultClonePath = "d:\\efault\\path")
        {
            os = os ?? Substitute.For<IOperatingSystem>();
            connectionManager = connectionManager ?? CreateConnectionManager("https://github.com");
            service = service ?? CreateRepositoryCloneService(defaultClonePath);
            gitHubTab = gitHubTab ?? CreateSelectViewModel();
            enterpriseTab = enterpriseTab ?? CreateSelectViewModel();
            urlTab = urlTab ?? Substitute.For<IRepositoryUrlViewModel>();

            return new RepositoryCloneViewModel(
                os,
                connectionManager,
                service,
                gitHubTab,
                enterpriseTab,
                urlTab);
        }

        static IRepositoryModel CreateRepositoryModel(string repo = "owner/repo")
        {
            var split = repo.Split('/');
            var (owner, name) = (split[0], split[1]);
            return CreateRepositoryModel(owner, name);
        }

        static IRepositoryModel CreateRepositoryModel(string owner, string name)
        {
            var repository = Substitute.For<IRepositoryModel>();
            repository.Owner.Returns(owner);
            repository.Name.Returns(name);
            return repository;
        }
    }
}
