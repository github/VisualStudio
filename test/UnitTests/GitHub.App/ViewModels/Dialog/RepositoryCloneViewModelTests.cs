using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.Validation;
using NSubstitute;
using Rothko;
using NUnit.Framework;
using GitHub.Collections;
using NSubstitute.Core;
using GitHub.Factories;
using GitHub.Primitives;
using GitHub.ViewModels.Dialog;
using System.Diagnostics;

public class RepositoryCloneViewModelTests
{
    const int Timeout = 2000;

    static RepositoryCloneViewModel GetVM(IModelService modelService, IRepositoryCloneService cloneService, IOperatingSystem os)
    {
        var connection = Substitute.For<IConnection>();
        connection.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);
        var modelServiceFactory = Substitute.For<IModelServiceFactory>();
        modelServiceFactory.CreateAsync(connection).Returns(modelService);

        var vm = new RepositoryCloneViewModel(
            modelServiceFactory,
            cloneService,
            os);
        vm.InitializeAsync(connection).Wait();
        return vm;
    }

    static ITrackingCollection<IRemoteRepositoryModel> SetupRepositories(
        CallInfo callInfo,
        IObservable<IRemoteRepositoryModel> repositories)
    {
        var collection = callInfo.Arg<ITrackingCollection<IRemoteRepositoryModel>>();
        collection.Listen(repositories);
        return collection;
    }

    static IRemoteRepositoryModel CreateMockRepositoryModel()
    {
        var result = Substitute.For<IRemoteRepositoryModel>();
        result.Equals(result).Returns(true);
        return result;
    }

    public class TheLoadRepositoriesCommand : TestBaseClass
    {
        [Test]
        public async Task LoadsRepositories()
        {
            var repos = new IRemoteRepositoryModel[]
            {
                CreateMockRepositoryModel(),
                CreateMockRepositoryModel(),
                CreateMockRepositoryModel(),
            };
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repos.ToObservable()));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());

            var col = (ITrackingCollection<IRemoteRepositoryModel>)vm.Repositories;
            await col.OriginalCompleted.Timeout(TimeSpan.FromMilliseconds(Timeout));
            Assert.That(3, Is.EqualTo(vm.Repositories.Count));
        }
    }

    public class TheIsBusyProperty : TestBaseClass
    {
        [Test]
        public async Task StartsTrueBecomesFalseWhenCompleted()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());
            var col = (ITrackingCollection<IRemoteRepositoryModel>)vm.Repositories;

            Assert.True(vm.IsBusy);

            var done = new ReplaySubject<Unit>();
            done.OnNext(Unit.Default);
            done.Subscribe();
            col.Subscribe(t => done?.OnCompleted(), () => { });

            repoSubject.OnNext(Substitute.For<IRemoteRepositoryModel>());
            repoSubject.OnNext(Substitute.For<IRemoteRepositoryModel>());

            await done.Timeout(TimeSpan.FromMilliseconds(Timeout));
            done = null;

            Assert.True(vm.IsBusy);

            repoSubject.OnCompleted();

            await col.OriginalCompleted.Timeout(TimeSpan.FromMilliseconds(Timeout));

            // we need to wait slightly because the subscription OnComplete in the model
            // runs right after the above await finishes, which means the assert
            // gets checked before the flag is set
            await Task.Delay(100);
            Assert.False(vm.IsBusy);
        }

        [Test]
        public void IsFalseWhenLoadingReposFailsImmediately()
        {
            var repoSubject = Observable.Throw<IRemoteRepositoryModel>(new InvalidOperationException("Doh!"));
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());

            Assert.True(vm.LoadingFailed);
            Assert.False(vm.IsBusy);
        }
    }

    public class TheNoRepositoriesFoundProperty : TestBaseClass
    {
        [Test]
        public void IsTrueInitially()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();

            var connection = Substitute.For<IConnection>();
            connection.HostAddress.Returns(HostAddress.GitHubDotComHostAddress);

            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));

            var modelServiceFactory = Substitute.For<IModelServiceFactory>();
            modelServiceFactory.CreateAsync(connection).Returns(modelService);

            var cloneService = Substitute.For<IRepositoryCloneService>();

            var vm = new RepositoryCloneViewModel(
                modelServiceFactory,
                cloneService,
                Substitute.For<IOperatingSystem>());

            Assert.False(vm.LoadingFailed);
            Assert.True(vm.NoRepositoriesFound);
        }

        [Test]
        public async Task IsFalseWhenLoadingAndCompletedWithRepository()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());

            repoSubject.OnNext(Substitute.For<IRemoteRepositoryModel>());

            Assert.False(vm.NoRepositoriesFound);

            repoSubject.OnCompleted();

            var col = (ITrackingCollection<IRemoteRepositoryModel>)vm.Repositories;
            await col.OriginalCompleted.Timeout(TimeSpan.FromMilliseconds(Timeout));
            Assert.That(vm.Repositories, Has.Count.EqualTo(1));
            Assert.False(vm.NoRepositoriesFound);
        }

        [Test]
        public void IsFalseWhenFailed()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());

            repoSubject.OnError(new InvalidOperationException());

            Assert.False(vm.NoRepositoriesFound);
        }

        [Test]
        public async Task IsTrueWhenLoadingCompleteNotFailedAndNoRepositories()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());

            repoSubject.OnCompleted();

            // we need to delay slightly because the subscribers listening for OnComplete
            // need to run before the assert is checked
            await Task.Delay(100);
            Assert.True(vm.NoRepositoriesFound);
        }
    }

    public class TheFilterTextEnabledProperty : TestBaseClass
    {
        [Test]
        public void IsTrueInitially()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();

            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());

            Assert.False(vm.LoadingFailed);
            Assert.True(vm.FilterTextIsEnabled);
        }

        [Test]
        public void IsFalseIfLoadingReposFails()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());

            Assert.False(vm.LoadingFailed);

            repoSubject.OnError(new InvalidOperationException("Doh!"));

            Assert.True(vm.LoadingFailed);
            Assert.False(vm.FilterTextIsEnabled);
            repoSubject.OnCompleted();
        }

        [Test]
        public async Task IsFalseWhenLoadingCompleteNotFailedAndNoRepositories()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());

            repoSubject.OnCompleted();

            // we need to delay slightly because the subscribers listening for OnComplete
            // need to run before the assert is checked
            await Task.Delay(100);
            Assert.False(vm.FilterTextIsEnabled);
        }
    }

    public class TheLoadingFailedProperty : TestBaseClass
    {
        [Test]
        public void IsTrueIfLoadingReposFails()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());

            Assert.False(vm.LoadingFailed);

            repoSubject.OnError(new InvalidOperationException("Doh!"));

            Assert.True(vm.LoadingFailed);
            Assert.False(vm.IsBusy);
            repoSubject.OnCompleted();
        }
    }

    public class TheBaseRepositoryPathValidator
    {
        [Test]
        public void IsInvalidWhenDestinationRepositoryExists()
        {
            var repo = Substitute.For<IRemoteRepositoryModel>();
            repo.Id.Returns(1);
            repo.Name.Returns("bar");
            var data = new[] { repo }.ToObservable();

            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, data));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var os = Substitute.For<IOperatingSystem>();
            var directories = Substitute.For<IDirectoryFacade>();
            os.Directory.Returns(directories);
            directories.Exists(@"c:\foo\bar").Returns(true);
            var vm = GetVM(
                modelService,
                cloneService,
                os);

            vm.BaseRepositoryPath = @"c:\foo";
            vm.SelectedRepository = repo;

            Assert.That(ValidationStatus.Invalid, Is.EqualTo(vm.BaseRepositoryPathValidator.ValidationResult.Status));
        }
    }

    public class TheCloneCommand : TestBaseClass
    {
        [Test]
        public void IsEnabledWhenRepositorySelectedAndPathValid()
        {
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, Observable.Empty<IRemoteRepositoryModel>()));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());
            Assert.False(vm.CloneCommand.CanExecute(null));

            vm.BaseRepositoryPath = @"c:\fake\path";
            vm.SelectedRepository = Substitute.For<IRemoteRepositoryModel>();

            Assert.True(vm.CloneCommand.CanExecute(null));
        }

        [Test]
        public void IsNotEnabledWhenPathIsNotValid()
        {
            var modelService = Substitute.For<IModelService>();
            modelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, Observable.Empty<IRemoteRepositoryModel>()));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                modelService,
                cloneService,
                Substitute.For<IOperatingSystem>());
            vm.BaseRepositoryPath = @"c:|fake\path";
            Assert.False(vm.CloneCommand.CanExecute(null));

            vm.SelectedRepository = Substitute.For<IRemoteRepositoryModel>();

            Assert.False(vm.CloneCommand.CanExecute(null));
        }
    }
}
