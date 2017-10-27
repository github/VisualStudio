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
using ReactiveUI;
using Rothko;
using System.Collections.ObjectModel;
using GitHub.Collections;
using GitHub.UI;
using GitHub.Extensions.Reactive;
using GitHub.Factories;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Clone)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCloneViewModel : DialogViewModelBase, IRepositoryCloneViewModel
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IConnection connection;
        readonly IModelServiceFactory modelServiceFactory;
        readonly IOperatingSystem operatingSystem;
        readonly ReactiveCommand<object> browseForDirectoryCommand = ReactiveCommand.Create();
        IModelService modelService;
        bool noRepositoriesFound;
        readonly ObservableAsPropertyHelper<bool> canClone;
        string baseRepositoryPath;
        bool loadingFailed;

        [ImportingConstructor]
        public RepositoryCloneViewModel(
            IGlobalConnection connection,
            IModelServiceFactory modelServiceFactory,
            IRepositoryCloneService cloneService,
            IOperatingSystem operatingSystem)
            : this(connection.Get(), modelServiceFactory, cloneService, operatingSystem)
        {
        }

        public RepositoryCloneViewModel(
            IConnection connection,
            IModelServiceFactory modelServiceFactory,
            IRepositoryCloneService cloneService,
            IOperatingSystem operatingSystem)
        {
            Guard.ArgumentNotNull(connection, nameof(connection));
            Guard.ArgumentNotNull(modelServiceFactory, nameof(modelServiceFactory));
            Guard.ArgumentNotNull(cloneService, nameof(cloneService));
            Guard.ArgumentNotNull(operatingSystem, nameof(operatingSystem));

            this.connection = connection;
            this.modelServiceFactory = modelServiceFactory;
            this.operatingSystem = operatingSystem;

            Title = string.Format(CultureInfo.CurrentCulture, Resources.CloneTitle, connection.HostAddress.Title);

            Repositories = new TrackingCollection<IRemoteRepositoryModel>();
            repositories.ProcessingDelay = TimeSpan.Zero;
            repositories.Comparer = OrderedComparer<IRemoteRepositoryModel>.OrderBy(x => x.Owner).ThenBy(x => x.Name).Compare;
            repositories.Filter = FilterRepository;
            repositories.NewerComparer = OrderedComparer<IRemoteRepositoryModel>.OrderByDescending(x => x.UpdatedAt).Compare;

            filterTextIsEnabled = this.WhenAny(x => x.IsBusy,
                loading => loading.Value || repositories.UnfilteredCount > 0 && !LoadingFailed)
                .ToProperty(this, x => x.FilterTextIsEnabled);

            this.WhenAny(
                x => x.repositories.UnfilteredCount,
                x => x.IsBusy,
                x => x.LoadingFailed,
                (unfilteredCount, loading, failed) =>
                {
                    if (loading.Value)
                        return false;

                    if (failed.Value)
                        return false;

                    return unfilteredCount.Value == 0;
                })
                .Subscribe(x =>
                {
                    NoRepositoriesFound = x;
                });

            this.WhenAny(x => x.FilterText, x => x.Value)
                .DistinctUntilChanged(StringComparer.OrdinalIgnoreCase)
                .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                .Subscribe(_ => repositories.Filter = FilterRepository);

            var baseRepositoryPath = this.WhenAny(
                x => x.BaseRepositoryPath,
                x => x.SelectedRepository,
                (x, y) => x.Value);

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
            CloneCommand = ReactiveCommand.Create(canCloneObservable);

            browseForDirectoryCommand.Subscribe(_ => ShowBrowseForDirectoryDialog());
            this.WhenAny(x => x.BaseRepositoryPathValidator.ValidationResult, x => x.Value)
                .Subscribe();
            BaseRepositoryPath = cloneService.DefaultClonePath;
            NoRepositoriesFound = true;
        }

        public override void Initialize(ViewWithData data)
        {
            base.Initialize(data);

            if (modelService == null)
            {
                modelService = modelServiceFactory.CreateBlocking(connection);
            }

            IsBusy = true;
            modelService.GetRepositories(repositories);
            repositories.OriginalCompleted
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(
                    _ => { }
                    , ex =>
                    {
                        LoadingFailed = true;
                        IsBusy = false;
                        log.Error("Error while loading repositories", ex);
                    },
                    () => IsBusy = false
            );
            repositories.Subscribe();
        }

        bool FilterRepository(IRemoteRepositoryModel repo, int position, IList<IRemoteRepositoryModel> list)
        {
            Guard.ArgumentNotNull(repo, nameof(repo));
            Guard.ArgumentNotNull(list, nameof(list));

            if (string.IsNullOrWhiteSpace(FilterText))
                return true;

            // Not matching on NameWithOwner here since that's already been filtered on by the selected account
            return repo.Name.IndexOf(FilterText ?? "", StringComparison.OrdinalIgnoreCase) != -1;
        }

        bool IsAlreadyRepoAtPath(string path)
        {
            Guard.ArgumentNotEmptyString(path, nameof(path));

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
            get { return baseRepositoryPath; }
            set { this.RaiseAndSetIfChanged(ref baseRepositoryPath, value); }
        }

        /// <summary>
        /// Signals that the user clicked the clone button.
        /// </summary>
        public IReactiveCommand<object> CloneCommand { get; private set; }

        TrackingCollection<IRemoteRepositoryModel> repositories;
        public ObservableCollection<IRemoteRepositoryModel> Repositories
        {
            get { return repositories; }
            private set { this.RaiseAndSetIfChanged(ref repositories, (TrackingCollection<IRemoteRepositoryModel>)value); }
        }

        IRepositoryModel selectedRepository;
        /// <summary>
        /// Selected repository to clone
        /// </summary>
        public IRepositoryModel SelectedRepository
        {
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
        public string FilterText
        {
            get { return filterText; }
            set { this.RaiseAndSetIfChanged(ref filterText, value); }
        }

        public bool LoadingFailed
        {
            get { return loadingFailed; }
            private set { this.RaiseAndSetIfChanged(ref loadingFailed, value); }
        }

        public bool NoRepositoriesFound
        {
            get { return noRepositoriesFound; }
            private set { this.RaiseAndSetIfChanged(ref noRepositoriesFound, value); }
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

        public override IObservable<Unit> Done => CloneCommand.SelectUnit();
    }
}
