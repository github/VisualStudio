using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using GitHub.App;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.Validation;
using NLog;
using NullGuard;
using ReactiveUI;
using Rothko;
using System.Collections.ObjectModel;
using GitHub.Collections;
using GitHub.UI;
using GitHub.Extensions.Reactive;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.StartPageClone)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class StartPageCloneViewModel : BaseViewModel, IBaseCloneViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IRepositoryCloneService cloneService;
        readonly IOperatingSystem operatingSystem;
        readonly INotificationService notificationService;
        readonly IUsageTracker usageTracker;
        readonly ReactiveCommand<object> browseForDirectoryCommand = ReactiveCommand.Create();
        readonly ObservableAsPropertyHelper<bool> canClone;
        string baseRepositoryPath;

        [ImportingConstructor]
        StartPageCloneViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            IRepositoryCloneService repositoryCloneService,
            IOperatingSystem operatingSystem,
            INotificationService notificationService,
            IUsageTracker usageTracker)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, repositoryCloneService, operatingSystem, notificationService, usageTracker)
        { }

        public StartPageCloneViewModel(
            IRepositoryHost repositoryHost,
            IRepositoryCloneService cloneService,
            IOperatingSystem operatingSystem,
            INotificationService notificationService,
            IUsageTracker usageTracker)
        {
            this.cloneService = cloneService;
            this.operatingSystem = operatingSystem;
            this.notificationService = notificationService;
            this.usageTracker = usageTracker;

            Title = string.Format(CultureInfo.CurrentCulture, Resources.CloneTitle, repositoryHost.Title);

            var baseRepositoryPath = this.WhenAnyValue(x => x.BaseRepositoryPath);

            BaseRepositoryPathValidator = ReactivePropertyValidator.ForObservable(baseRepositoryPath)
                .IfNullOrEmpty(Resources.RepositoryCreationClonePathEmpty)
                .IfTrue(x => x.Length > 200, Resources.RepositoryCreationClonePathTooLong)
                .IfContainsInvalidPathChars(Resources.RepositoryCreationClonePathInvalidCharacters)
                .IfPathNotRooted(Resources.RepositoryCreationClonePathInvalid)
                .IfTrue(IsAlreadyRepoAtPath, Resources.RepositoryNameValidatorAlreadyExists);

            var canCloneObservable = this.WhenAny(
                x => x.SelectedRepository,
                x => x.BaseRepositoryPathValidator.ValidationResult.IsValid,
                (x, y) => x.Value != null && y.Value);
            canClone = canCloneObservable.ToProperty(this, x => x.CanClone);
            CloneCommand = ReactiveCommand.CreateAsyncObservable(canCloneObservable, OnCloneRepository);

            browseForDirectoryCommand.Subscribe(_ => ShowBrowseForDirectoryDialog());
            this.WhenAny(x => x.BaseRepositoryPathValidator.ValidationResult, x => x.Value)
                .Subscribe();
            BaseRepositoryPath = cloneService.DefaultClonePath;
        }


        IObservable<Unit> OnCloneRepository(object state)
        {
            return Observable.Start(() =>
            {
                var repository = SelectedRepository;
                Debug.Assert(repository != null, "Should not be able to attempt to clone a repo when it's null");
                if (repository == null)
                {
                    notificationService.ShowError(Resources.RepositoryCloneFailedNoSelectedRepo);
                    return Observable.Return(Unit.Default);
                }

                // The following is a noop if the directory already exists.
                operatingSystem.Directory.CreateDirectory(BaseRepositoryPath);

                return cloneService.CloneRepository(repository.CloneUrl, repository.Name, BaseRepositoryPath)
                    .ContinueAfter(() =>
                    {
                        usageTracker.IncrementCloneCount().Forget();
                        return Observable.Return(Unit.Default);
                    });
            })
            .SelectMany(_ => _)
            .Catch<Unit, Exception>(e =>
            {
                var repository = SelectedRepository;
                Debug.Assert(repository != null, "Should not be able to attempt to clone a repo when it's null");
                notificationService.ShowError(e.GetUserFriendlyErrorMessage(ErrorType.ClonedFailed, repository.Name));
                return Observable.Return(Unit.Default);
            });
        }

        bool IsAlreadyRepoAtPath(string path)
        {
            Debug.Assert(path != null, "RepositoryCloneViewModel.IsAlreadyRepoAtPath cannot be passed null as a path parameter.");

            bool isAlreadyRepoAtPath = false;

            if (SelectedRepository != null)
            {
                string potentialPath = Path.Combine(path, SelectedRepository.Name);
                isAlreadyRepoAtPath = operatingSystem.Directory.Exists(potentialPath);
            }

            return isAlreadyRepoAtPath;
        }

        IObservable<Unit> ShowBrowseForDirectoryDialog()
        {
            return Observable.Start(() =>
            {
                // We store this in a local variable to prevent it changing underneath us while the
                // folder dialog is open.
                var localBaseRepositoryPath = BaseRepositoryPath;
                var browseResult = operatingSystem.Dialog.BrowseForDirectory(localBaseRepositoryPath, Resources.BrowseForDirectory);

                if (!browseResult.Success)
                    return;

                var directory = browseResult.DirectoryPath ?? localBaseRepositoryPath;

                try
                {
                    BaseRepositoryPath = directory;
                }
                catch (Exception e)
                {
                    // TODO: We really should limit this to exceptions we know how to handle.
                    log.Error(string.Format(CultureInfo.InvariantCulture,
                        "Failed to set base repository path.{0}localBaseRepositoryPath = \"{1}\"{0}BaseRepositoryPath = \"{2}\"{0}Chosen directory = \"{3}\"",
                        System.Environment.NewLine, localBaseRepositoryPath ?? "(null)", BaseRepositoryPath ?? "(null)", directory ?? "(null)"), e);
                }
            }, RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// Path to clone repositories into
        /// </summary>
        public string BaseRepositoryPath
        {
            [return: AllowNull]
            get { return baseRepositoryPath; }
            set { this.RaiseAndSetIfChanged(ref baseRepositoryPath, value); }
        }

        /// <summary>
        /// Fires off the cloning process
        /// </summary>
        public IReactiveCommand<Unit> CloneCommand { get; private set; }

        ISimpleRepositoryModel selectedRepository;
        /// <summary>
        /// Selected repository to clone
        /// </summary>
        [AllowNull]
        public ISimpleRepositoryModel SelectedRepository
        {
            [return: AllowNull]
            get { return selectedRepository; }
            set { this.RaiseAndSetIfChanged(ref selectedRepository, value); }
        }

        public ICommand BrowseForDirectory
        {
            get { return browseForDirectoryCommand; }
        }

        public bool CanClone
        {
            get { return canClone.Value; }
        }

        public ReactivePropertyValidator<string> BaseRepositoryPathValidator
        {
            get;
            private set;
        }
    }
}
