using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Reactive;
using System.Reactive.Disposables;
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
using GitHub.Collections;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Clone)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCloneViewModel : BaseViewModel, IRepositoryCloneViewModel, IDisposable
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IRepositoryHost repositoryHost;
        readonly IRepositoryCloneService cloneService;
        readonly IOperatingSystem operatingSystem;
        readonly IVSServices vsServices;
        //readonly IReactiveCommand<IReadOnlyList<IRepositoryModel>> loadRepositoriesCommand;
        readonly ReactiveCommand<object> browseForDirectoryCommand = ReactiveCommand.Create();
        readonly ObservableAsPropertyHelper<bool> noRepositoriesFound;
        readonly ObservableAsPropertyHelper<bool> canClone;
        readonly CompositeDisposable disposables = new CompositeDisposable();
        string baseRepositoryPath;
        bool loadingFailed;
        bool isLoading;

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

            SetupCollection();

            noRepositoriesFound = this.WhenAny(x => x.FilterTextIsEnabled, v => !v.Value)
                .ToProperty(this, x => x.NoRepositoriesFound);

            this.WhenAny(x => x.FilterText, x => x.Value)
                .DistinctUntilChanged(StringComparer.OrdinalIgnoreCase)
                .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                .Do(filter => Repositories.SetFilter((r, index, l) => FilterRepository(r)))
                .Subscribe();

            BaseRepositoryPathValidator = this.CreateBaseRepositoryPathValidator();

            var canCloneObservable = this.WhenAny(
                x => x.SelectedRepository,
                x => x.BaseRepositoryPathValidator.ValidationResult.IsValid,
                (x, y) => x.Value != null && y.Value);
            canClone = canCloneObservable.ToProperty(this, x => x.CanClone);
            CloneCommand = ReactiveCommand.CreateAsyncObservable(canCloneObservable, OnCloneRepository);

            var canRefresh = this.WhenAny(x => x.IsLoading, v => !v.Value);
            RefreshCommand = ReactiveCommand.CreateAsyncObservable(canRefresh, _ => SetupCollection(true));

            browseForDirectoryCommand.Subscribe(_ => ShowBrowseForDirectoryDialog());
            this.WhenAny(x => x.BaseRepositoryPathValidator.ValidationResult, x => x.Value)
                .Subscribe();
            BaseRepositoryPath = cloneService.DefaultClonePath;
        }

        IObservable<Unit> SetupCollection(bool forceRefresh = false)
        {
            disposables.Clear();

            IsLoading = LoadingFailed = FilterTextIsEnabled = false;

            Repositories = repositoryHost.ModelService.GetRepositories(forceRefresh);
            disposables.Add(Repositories);

            Repositories.CurrentState
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(s => IsLoading = s == TrackingCollectionState.Loading)
                .Subscribe();

            Repositories.CurrentState
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(s => LoadingFailed = s == TrackingCollectionState.Failed)
                .Subscribe();

            disposables.Add(Repositories.Subscribe(_ => FilterTextIsEnabled = FilterTextIsEnabled || Repositories.Count > 0, () => { }));

            return Observable.Return(Unit.Default);
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
        /// <summary>
        /// Refreshes the list of repositories from the server
        /// </summary>
        public IReactiveCommand<Unit> RefreshCommand { get; private set; }

        ITrackingCollection<IRepositoryModel> repositories;
        /// <summary>
        /// List of repositories as returned by the server
        /// </summary>
        public ITrackingCollection<IRepositoryModel> Repositories
        {
            get { return repositories; }
            private set { this.RaiseAndSetIfChanged(ref repositories, value); }
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

        bool filterTextIsEnabled;
        /// <summary>
        /// True if there are repositories (otherwise no point in filtering)
        /// </summary>
        public bool FilterTextIsEnabled {
            get { return filterTextIsEnabled; }
            set { this.RaiseAndSetIfChanged(ref filterTextIsEnabled, value); }
        }

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
            get { return isLoading; }
            set { this.RaiseAndSetIfChanged(ref isLoading, value); }
        }

        //public IReactiveCommand<IReadOnlyList<IRepositoryModel>> LoadRepositoriesCommand
        //{
        //    get { return loadRepositoriesCommand; }
        //}

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

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    disposed = true;
                    disposables.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
