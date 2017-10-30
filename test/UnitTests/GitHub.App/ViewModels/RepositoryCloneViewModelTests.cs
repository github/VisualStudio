using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.Validation;
using GitHub.ViewModels;
using NSubstitute;
using Rothko;
using Xunit;
using GitHub.Collections;
using NSubstitute.Core;

public class RepositoryCloneViewModelTests
{
    static RepositoryCloneViewModel GetVM(IRepositoryHost repositoryHost, IRepositoryCloneService cloneService,
        IOperatingSystem os, INotificationService notificationService, IUsageTracker usageTracker)
    {
        var vm = new RepositoryCloneViewModel(
            repositoryHost,
            cloneService,
            os);
        vm.Initialize(null);
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
        [Fact]
        public async Task LoadsRepositories()
        {
            var repos = new IRemoteRepositoryModel[]
            {
                CreateMockRepositoryModel(),
                CreateMockRepositoryModel(),
                CreateMockRepositoryModel(),
            };
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repos.ToObservable()));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());

            var col = (ITrackingCollection<IRemoteRepositoryModel>)vm.Repositories;
            await col.OriginalCompleted;
            Assert.Equal(3, vm.Repositories.Count);
        }
    }

    public class TheIsBusyProperty : TestBaseClass
    {
        [Fact]
        public async Task StartsTrueBecomesFalseWhenCompleted()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());
            var col = (ITrackingCollection<IRemoteRepositoryModel>)vm.Repositories;

            Assert.True(vm.IsBusy);

            var done = new ReplaySubject<Unit>();
            done.OnNext(Unit.Default);
            done.Subscribe();
            col.Subscribe(t => done?.OnCompleted(), () => { });

            repoSubject.OnNext(Substitute.For<IRemoteRepositoryModel>());
            repoSubject.OnNext(Substitute.For<IRemoteRepositoryModel>());

            await done;
            done = null;

            Assert.True(vm.IsBusy);

            repoSubject.OnCompleted();

            await col.OriginalCompleted;

            Assert.False(vm.IsBusy);
        }

        [Fact]
        public void IsFalseWhenLoadingReposFailsImmediately()
        {
            var repoSubject = Observable.Throw<IRemoteRepositoryModel>(new InvalidOperationException("Doh!"));
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());

            Assert.True(vm.LoadingFailed);
            Assert.False(vm.IsBusy);
        }
    }

    public class TheNoRepositoriesFoundProperty : TestBaseClass
    {
        [Fact]
        public void IsTrueInitially()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();

            var vm = new RepositoryCloneViewModel(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>());

            Assert.False(vm.LoadingFailed);
            Assert.True(vm.NoRepositoriesFound);
        }

        [Fact]
        public async Task IsFalseWhenLoadingAndCompletedWithRepository()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());

            repoSubject.OnNext(Substitute.For<IRemoteRepositoryModel>());

            Assert.False(vm.NoRepositoriesFound);

            repoSubject.OnCompleted();

            var col = (ITrackingCollection<IRemoteRepositoryModel>)vm.Repositories;
            await col.OriginalCompleted;
            await Task.Delay(100);
            Assert.Equal(1, vm.Repositories.Count);
            Assert.False(vm.NoRepositoriesFound);
        }

        [Fact]
        public void IsFalseWhenFailed()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());

            repoSubject.OnError(new InvalidOperationException());

            Assert.False(vm.NoRepositoriesFound);
        }

        [Fact]
        public void IsTrueWhenLoadingCompleteNotFailedAndNoRepositories()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());

            repoSubject.OnCompleted();

            Assert.True(vm.NoRepositoriesFound);
        }
    }

    public class TheFilterTextEnabledProperty : TestBaseClass
    {
        [Fact]
        public void IsTrueInitially()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();

            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());

            Assert.False(vm.LoadingFailed);
            Assert.True(vm.FilterTextIsEnabled);
        }

        [Fact]
        public void IsFalseIfLoadingReposFails()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());

            Assert.False(vm.LoadingFailed);

            repoSubject.OnError(new InvalidOperationException("Doh!"));

            Assert.True(vm.LoadingFailed);
            Assert.False(vm.FilterTextIsEnabled);
            repoSubject.OnCompleted();
        }

        [Fact]
        public void IsFalseWhenLoadingCompleteNotFailedAndNoRepositories()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());

            repoSubject.OnCompleted();

            Assert.False(vm.FilterTextIsEnabled);
        }
    }

    public class TheLoadingFailedProperty : TestBaseClass
    {
        [Fact]
        public void IsTrueIfLoadingReposFails()
        {
            var repoSubject = new Subject<IRemoteRepositoryModel>();
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, repoSubject));
            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());

            Assert.False(vm.LoadingFailed);

            repoSubject.OnError(new InvalidOperationException("Doh!"));

            Assert.True(vm.LoadingFailed);
            Assert.False(vm.IsBusy);
            repoSubject.OnCompleted();
        }
    }

    public class TheBaseRepositoryPathValidator
    {
        [Fact]
        public void IsInvalidWhenDestinationRepositoryExists()
        {
            var repo = Substitute.For<IRemoteRepositoryModel>();
            repo.Id.Returns(1);
            repo.Name.Returns("bar");
            var data = new[] { repo }.ToObservable();

            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, data));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var os = Substitute.For<IOperatingSystem>();
            var directories = Substitute.For<IDirectoryFacade>();
            os.Directory.Returns(directories);
            directories.Exists(@"c:\foo\bar").Returns(true);
            var vm = GetVM(
                repositoryHost,
                cloneService,
                os,
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());

            vm.BaseRepositoryPath = @"c:\foo";
            vm.SelectedRepository = repo;

            Assert.Equal(ValidationStatus.Invalid, vm.BaseRepositoryPathValidator.ValidationResult.Status);
        }
    }

    public class TheCloneCommand : TestBaseClass
    {
        [Fact]
        public void IsEnabledWhenRepositorySelectedAndPathValid()
        {
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, Observable.Empty<IRemoteRepositoryModel>()));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());
            Assert.False(vm.CloneCommand.CanExecute(null));

            vm.BaseRepositoryPath = @"c:\fake\path";
            vm.SelectedRepository = Substitute.For<IRemoteRepositoryModel>();

            Assert.True(vm.CloneCommand.CanExecute(null));
        }

        [Fact]
        public void IsNotEnabledWhenPathIsNotValid()
        {
            var repositoryHost = Substitute.For<IRepositoryHost>();
            repositoryHost.ModelService.GetRepositories(Arg.Any<ITrackingCollection<IRemoteRepositoryModel>>())
                .Returns(x => SetupRepositories(x, Observable.Empty<IRemoteRepositoryModel>()));

            var cloneService = Substitute.For<IRepositoryCloneService>();
            var vm = GetVM(
                repositoryHost,
                cloneService,
                Substitute.For<IOperatingSystem>(),
                Substitute.For<INotificationService>(),
                Substitute.For<IUsageTracker>());
            vm.BaseRepositoryPath = @"c:|fake\path";
            Assert.False(vm.CloneCommand.CanExecute(null));

            vm.SelectedRepository = Substitute.For<IRemoteRepositoryModel>();

            Assert.False(vm.CloneCommand.CanExecute(null));
        }
    }
}
