using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Rothko;
using Xunit;

public class RepositoryCloneViewModelTests
{
    public class TheLoadRepositoriesCommand : TestBaseClass
    {
        [Fact]
        public async Task LoadsRepositories()
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

            await vm.LoadRepositoriesCommand.ExecuteAsync();

            Assert.Equal(3, vm.FilteredRepositories.Count);
        }
    }

    public class TheIsLoadingProperty : TestBaseClass
    {
        [Fact]
        public void StartsTrueBecomesFalseWhenCompleted()
        {
            var repoSubject = new Subject<IRepositoryModel[]>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories().Returns(repoSubject);
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());

            Assert.False(vm.IsLoading);

            vm.LoadRepositoriesCommand.ExecuteAsync().Subscribe();

            Assert.True(vm.IsLoading);

            repoSubject.OnNext(new[] { Substitute.For<IRepositoryModel>() });
            repoSubject.OnNext(new[] { Substitute.For<IRepositoryModel>() });

            Assert.True(vm.IsLoading);

            repoSubject.OnCompleted();

            Assert.False(vm.IsLoading);
        }

        [Fact]
        public void IsFalseWhenLoadingReposFailsImmediately()
        {
            var repoSubject = Observable.Throw<IRepositoryModel[]>(new InvalidOperationException("Doh!"));
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories().Returns(repoSubject);
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());

            vm.LoadRepositoriesCommand.ExecuteAsync().Subscribe();

            Assert.True(vm.LoadingFailed);
            Assert.False(vm.IsLoading);
        }
    }

    public class TheNoRepositoriesFoundProperty : TestBaseClass
    {
        [Fact]
        public void IsTrueInitially()
        {
            var repoSubject = new Subject<IRepositoryModel[]>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories().Returns(repoSubject);
            var cloneService = Substitute.For<IRepositoryCloneService>();

            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());

            Assert.True(vm.NoRepositoriesFound);
        }

        [Fact]
        public void IsFalseWhenLoadingAndCompletedWithRepository()
        {
            var repoSubject = new Subject<IRepositoryModel[]>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories().Returns(repoSubject);
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());
            vm.LoadRepositoriesCommand.ExecuteAsync().Subscribe();

            repoSubject.OnNext(new[] { Substitute.For<IRepositoryModel>() });

            Assert.False(vm.NoRepositoriesFound);

            repoSubject.OnCompleted();

            Assert.Equal(1, vm.FilteredRepositories.Count);
            Assert.False(vm.NoRepositoriesFound);
        }

        [Fact]
        public void IsFalseWhenFailed()
        {
            var repoSubject = new Subject<IRepositoryModel[]>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories().Returns(repoSubject);
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());
            vm.LoadRepositoriesCommand.ExecuteAsync().Subscribe();

            repoSubject.OnError(new InvalidOperationException());

            Assert.False(vm.NoRepositoriesFound);
        }

        [Fact]
        public void IsTrueWhenLoadingCompleteNotFailedAndNoRepositories()
        {
            var repoSubject = new Subject<IRepositoryModel[]>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories().Returns(repoSubject);
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());
            vm.LoadRepositoriesCommand.ExecuteAsync().Subscribe();

            repoSubject.OnCompleted();

            Assert.True(vm.NoRepositoriesFound);
        }
    }

    public class TheLoadingFailedProperty : TestBaseClass
    {
        [Fact]
        public void IsTrueIfLoadingReposFails()
        {
            var repoSubject = new Subject<IRepositoryModel[]>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories().Returns(repoSubject);
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());
            vm.LoadRepositoriesCommand.ExecuteAsync().Subscribe();

            Assert.False(vm.LoadingFailed);

            repoSubject.OnError(new InvalidOperationException("Doh!"));

            Assert.True(vm.LoadingFailed);
            Assert.False(vm.IsLoading);
        }
    }

    public class TheCloneCommand : TestBaseClass
    {
        [Fact]
        public void IsEnabledWhenRepositorySelectedAndPathValid()
        {
            var repositoryHost = Substitute.For<IRepositoryHost>();
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());
            Assert.False(vm.CloneCommand.CanExecute(null));

            vm.BaseRepositoryPath = @"c:\fake\path";
            vm.SelectedRepository = Substitute.For<IRepositoryModel>();

            Assert.True(vm.CloneCommand.CanExecute(null));
        }

        [Fact]
        public void IsNotEnabledWhenPathIsNotValid()
        {
            var repositoryHost = Substitute.For<IRepositoryHost>();
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<IVSServices>());
            vm.BaseRepositoryPath = @"c:|fake\path";
            Assert.False(vm.CloneCommand.CanExecute(null));

            vm.SelectedRepository = Substitute.For<IRepositoryModel>();

            Assert.False(vm.CloneCommand.CanExecute(null));
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
