using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.Validation;
using NLog;
using NullGuard;
using ReactiveUI;
using Rothko;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Clone)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCloneViewModel : BaseViewModel, IRepositoryCloneViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IRepositoryHost repositoryHost;
        readonly IRepositoryCloneService cloneService;
        readonly IOperatingSystem operatingSystem;
        readonly IVSServices vsServices;
        readonly IReactiveCommand<IReadOnlyList<IRepositoryModel>> loadRepositoriesCommand;
        readonly ReactiveCommand<object> browseForDirectoryCommand = ReactiveCommand.Create();
        readonly ObservableAsPropertyHelper<bool> isLoading;
        readonly ObservableAsPropertyHelper<bool> noRepositoriesFound;
        readonly ObservableAsPropertyHelper<bool> canClone;
        string baseRepositoryPath;
        bool loadingFailed;

        [ImportingConstructor]
        RepositoryCloneViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            IRepositoryCloneService repositoryCloneService,
            IOperatingSystem operatingSystem,
            IVSServices vsServices)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, repositoryCloneService, operatingSystem, vsServices)
        { }
        
        public RepositoryCloneViewModel(
            IRepositoryHost repositoryHost,
            IRepositoryCloneService cloneService,
            IOperatingSystem operatingSystem,
            IVSServices vsServices)
        {
            this.repositoryHost = repositoryHost;
            this.cloneService = cloneService;
            this.operatingSystem = operatingSystem;
            this.vsServices = vsServices;

            Title = string.Format(CultureInfo.CurrentCulture, Resources.CloneTitle, repositoryHost.Title);
            Repositories = new ReactiveList<IRepositoryModel>();
            loadRepositoriesCommand = ReactiveCommand.CreateAsyncObservable(OnLoadRepositories);
            isLoading = this.WhenAny(x => x.LoadingFailed, x => x.Value)
                .CombineLatest(loadRepositoriesCommand.IsExecuting, (failed, loading) => !failed && loading)
                .ToProperty(this, x => x.IsLoading);
            loadRepositoriesCommand.Subscribe(Repositories.AddRange);
            filterTextIsEnabled = this.WhenAny(x => x.Repositories.Count, x => x.Value > 0)
                .ToProperty(this, x => x.FilterTextIsEnabled);
            noRepositoriesFound = this.WhenAny(x => x.FilterTextIsEnabled, x => x.IsLoading, x => x.LoadingFailed
                , (any, loading, failed) => !any.Value && !loading.Value && !failed.Value)
                .ToProperty(this, x => x.NoRepositoriesFound);

            var filterResetSignal = this.WhenAny(x => x.FilterText, x => x.Value)
                .DistinctUntilChanged(StringComparer.OrdinalIgnoreCase)
                .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler);

            FilteredRepositories = Repositories.CreateDerivedCollection(
                x => x,
                filter: FilterRepository,
                signalReset: filterResetSignal
            );

            BaseRepositoryPathValidator = this.CreateBaseRepositoryPathValidator();

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

        IObservable<IReadOnlyList<IRepositoryModel>> OnLoadRepositories(object value)
        {
            return repositoryHost.ModelService.GetRepositories()
                .Catch<IReadOnlyList<IRepositoryModel>, Exception>(ex =>
                {
                    log.Error("Error while loading repositories", ex);
                    return Observable.Start(() => LoadingFailed = true, RxApp.MainThreadScheduler)
                        .Select(_ => new IRepositoryModel[] { });
                });
        }
       
        bool FilterRepository(IRepositoryModel repo)
        {
            if (string.IsNullOrWhiteSpace(FilterText))
                return true;

            // Not matching on NameWithOwner here since that's already been filtered on by the selected account
            return repo.Name.IndexOf(FilterText ?? "", StringComparison.OrdinalIgnoreCase) != -1;
        }

        IObservable<Unit> OnCloneRepository(object state)
        {
            return Observable.Start(() =>
            {
                var repository = SelectedRepository;
                Debug.Assert(repository != null, "Should not be able to attempt to clone a repo when it's null");
                // The following is a noop if the directory already exists.
                operatingSystem.Directory.CreateDirectory(BaseRepositoryPath);
                return cloneService.CloneRepository(repository.CloneUrl, repository.Name, BaseRepositoryPath);
            })
            .SelectMany(_ => _)
            .Catch<Unit, Exception>(e =>
            {
                var repository = SelectedRepository;
                Debug.Assert(repository != null, "Should not be able to attempt to clone a repo when it's null");
                vsServices.ShowError(e.GetUserFriendlyErrorMessage(ErrorType.ClonedFailed, repository.Name));
                return Observable.Return(Unit.Default);
            });
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

        IReactiveList<IRepositoryModel> repositories;
        /// <summary>
        /// List of repositories as returned by the server
        /// </summary>
        public IReactiveList<IRepositoryModel> Repositories
        {
            get { return repositories; }
            private set { this.RaiseAndSetIfChanged(ref repositories, value); }
        }

        IReactiveDerivedList<IRepositoryModel> filteredRepositories;
        /// <summary>
        /// List of repositories as filtered by user
        /// </summary>
        public IReactiveDerivedList<IRepositoryModel> FilteredRepositories
        {
            get { return filteredRepositories; }
            private set { this.RaiseAndSetIfChanged(ref filteredRepositories, value); }
        }

        IRepositoryModel selectedRepository;
        /// <summary>
        /// Selected repository to clone
        /// </summary>
        [AllowNull]
        public IRepositoryModel SelectedRepository
        {
            [return: AllowNull]
            get { return selectedRepository; }
            set { this.RaiseAndSetIfChanged(ref selectedRepository, value); }
        }

        readonly ObservableAsPropertyHelper<bool> filterTextIsEnabled;
        /// <summary>
        /// True if there are repositories (otherwise no point in filtering)
        /// </summary>
        public bool FilterTextIsEnabled { get { return filterTextIsEnabled.Value; } }

        string filterText;
        /// <summary>
        /// User text to filter the repositories list
        /// </summary>
        [AllowNull]
        public string FilterText
        {
            [return: AllowNull]
            get { return filterText; }
            set { this.RaiseAndSetIfChanged(ref filterText, value); }
        }

        public bool IsLoading
        {
            get { return isLoading.Value; }
        }

        public IReactiveCommand<IReadOnlyList<IRepositoryModel>> LoadRepositoriesCommand
        {
            get { return loadRepositoriesCommand; }
        }

        public bool LoadingFailed
        {
            get { return loadingFailed; }
            private set { this.RaiseAndSetIfChanged(ref loadingFailed, value); }
        }

        public bool NoRepositoriesFound
        {
            get { return noRepositoriesFound.Value; }
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
    }
}
