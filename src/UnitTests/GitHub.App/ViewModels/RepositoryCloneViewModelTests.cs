using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Rothko;
using Xunit;

public class RepositoryCloneViewModelTests
{
    public class TheCtor
    {
        [Fact]
        public void LoadsRepositories()
        {
            var repos = new IRepositoryModel[]
            {
                Substitute.For<IRepositoryModel>(),
                Substitute.For<IRepositoryModel>(),
                Substitute.For<IRepositoryModel>()
            };
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories().Returns(Observable.Return(repos));
            var cloneService = Substitute.For<IRepositoryCloneService>();

            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());

            Assert.Equal(3, vm.FilteredRepositories.Count);
        }
    }

    public class TheCloneCommand
    {
        [Fact]
        public void IsEnabledWhenRepositorySelected()
        {
            var repositoryHost = Substitute.For<IRepositoryHost>();
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());

            Assert.False(vm.CloneCommand.CanExecute(null));

            vm.SelectedRepository = Substitute.For<IRepositoryModel>();

            Assert.True(vm.CloneCommand.CanExecute(null));
        }

        [Fact]
        public async Task DisplaysErrorMessageWhenExceptionOccurs()
        {
            var repositoryHost = Substitute.For<IRepositoryHost>();
            var cloneService = Substitute.For<IRepositoryCloneService>();
            cloneService.CloneRepository(Args.String, Args.String, Args.String)
                .Returns(Observable.Throw<Unit>(new InvalidOperationException("Oh my! That was bad.")));
            var vsServices = Substitute.For<IVSServices>();
            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                vsServices);
            vm.BaseRepositoryPath = @"c:\fake";
            var repository = Substitute.For<IRepositoryModel>();
            repository.Name.Returns("octokit");
            vm.SelectedRepository = repository;

            await vm.CloneCommand.ExecuteAsync(null);

            vsServices.Received().ShowError(@"Failed to clone the repository 'octokit'
Email support@github.com if you continue to have problems.");
        }
    }
}
