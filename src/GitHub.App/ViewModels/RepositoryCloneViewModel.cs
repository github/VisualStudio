using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using NullGuard;
using ReactiveUI;
using Rothko;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Clone)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCloneViewModel : BaseViewModel, IRepositoryCloneViewModel
    {
        readonly IRepositoryCloneService cloneService;
        readonly IOperatingSystem operatingSystem;
        readonly IVSServices vsServices;

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
            this.cloneService = cloneService;
            this.operatingSystem = operatingSystem;
            this.vsServices = vsServices;

            Title = string.Format(CultureInfo.CurrentCulture, "Clone a {0} Repository", repositoryHost.Title);
            Repositories = new ReactiveList<IRepositoryModel>();
            repositoryHost.ModelService.GetRepositories()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(Repositories.AddRange);

            filterTextIsEnabled = this.WhenAny(x => x.Repositories.Count, x => x.Value > 0)
                .ToProperty(this, x => x.FilterTextIsEnabled);

            var filterResetSignal = this.WhenAny(x => x.FilterText, x => x.Value)
                .DistinctUntilChanged(StringComparer.OrdinalIgnoreCase)
                .Throttle(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler);

            FilteredRepositories = Repositories.CreateDerivedCollection(
                x => x,
                filter: FilterRepository,
                signalReset: filterResetSignal
            );

            var canClone = this.WhenAny(x => x.SelectedRepository, x => x.Value)
                .Select(repo => repo != null);
            CloneCommand = ReactiveCommand.CreateAsyncObservable(canClone, OnCloneRepository);

            BaseRepositoryPath = cloneService.GetLocalClonePathFromGitProvider(cloneService.DefaultClonePath);
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
                vsServices.ShowError(e.GetUserFriendlyErrorMessage(ErrorType.ClonedFailed, SelectedRepository.Name));
                return Observable.Return(Unit.Default);
            });
        }

        string baseRepositoryPath;
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
    }
}
