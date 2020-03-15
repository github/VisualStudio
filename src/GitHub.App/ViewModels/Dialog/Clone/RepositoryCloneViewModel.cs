using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;
using Rothko;
using Serilog;

namespace GitHub.ViewModels.Dialog.Clone
{
    [Export(typeof(IRepositoryCloneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RepositoryCloneViewModel : ViewModelBase, IRepositoryCloneViewModel
    {
        readonly IOperatingSystem os;
        readonly IConnectionManager connectionManager;
        readonly IRepositoryCloneService service;
        readonly IGitService gitService;
        readonly IUsageTracker usageTracker;
        readonly IDialogService dialogService;
        readonly IReadOnlyList<IRepositoryCloneTabViewModel> tabs;
        string path;
        UriString url;
        RepositoryModel previousRepository;
        ObservableAsPropertyHelper<string> pathWarning;
        int selectedTabIndex;

        [ImportingConstructor]
        public RepositoryCloneViewModel(
            IOperatingSystem os,
            IConnectionManager connectionManager,
            IRepositoryCloneService service,
            IGitService gitService,
            IUsageTracker usageTracker,
            IDialogService dialogService,
            IRepositorySelectViewModel gitHubTab,
            IRepositorySelectViewModel enterpriseTab)
        {
            this.os = os;
            this.connectionManager = connectionManager;
            this.service = service;
            this.gitService = gitService;
            this.usageTracker = usageTracker;
            this.dialogService = dialogService;

            GitHubTab = gitHubTab;
            EnterpriseTab = enterpriseTab;
            tabs = new IRepositoryCloneTabViewModel[] { GitHubTab, EnterpriseTab };

            var repository = this.WhenAnyValue(x => x.SelectedTabIndex)
                .SelectMany(x => tabs[x].WhenAnyValue(tab => tab.Repository));

            Path = service.DefaultClonePath;
            repository.Subscribe(x => UpdatePath(x));

            pathWarning = Observable.CombineLatest(
                repository,
                this.WhenAnyValue(x => x.Path),
                ValidatePathWarning)
                .ToProperty(this, x => x.PathWarning);

            var canClone = Observable.CombineLatest(
                repository, this.WhenAnyValue(x => x.Path),
                (repo, path) => repo != null && !service.DestinationFileExists(path) &&
                (!service.DestinationDirectoryExists(path) || service.DestinationDirectoryEmpty(path)));

            var canOpen = Observable.CombineLatest(
                repository, this.WhenAnyValue(x => x.Path),
                (repo, path) => repo != null && !service.DestinationFileExists(path) && service.DestinationDirectoryExists(path)
                && !service.DestinationDirectoryEmpty(path));

            Browse = ReactiveCommand.Create(() => BrowseForDirectory());
            Clone = ReactiveCommand.CreateFromObservable(
                () => repository.Select(x => new CloneDialogResult(Path, x?.CloneUrl)),
                canClone);
            Open = ReactiveCommand.CreateFromObservable(
                () => repository.Select(x => new CloneDialogResult(Path, x?.CloneUrl)),
                canOpen);

            LoginAsDifferentUser = ReactiveCommand.CreateFromTask(LoginAsDifferentUserAsync);
        }

        public IRepositorySelectViewModel GitHubTab { get; }
        public IRepositorySelectViewModel EnterpriseTab { get; }

        public string Path
        {
            get => path;
            set => this.RaiseAndSetIfChanged(ref path, value);
        }

        public UriString Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
        }

        public string PathWarning => pathWarning.Value;

        public int SelectedTabIndex
        {
            get => selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref selectedTabIndex, value);
        }

        public string Title => Resources.OpenFromGitHubTitle;

        public IObservable<object> Done => Observable.Merge(Clone, Open);

        public ReactiveCommand<Unit, Unit> LoginAsDifferentUser { get; }

        public ReactiveCommand<Unit, Unit> Browse { get; }

        public ReactiveCommand<Unit, CloneDialogResult> Clone { get; }

        public ReactiveCommand<Unit, CloneDialogResult> Open { get; }

        public async Task InitializeAsync(IConnection connection)
        {
            var connections = await connectionManager.GetLoadedConnections().ConfigureAwait(false);
            var gitHubConnection = connections.FirstOrDefault(x => x.HostAddress.IsGitHubDotCom());
            var enterpriseConnection = connections.FirstOrDefault(x => !x.HostAddress.IsGitHubDotCom());

            if (gitHubConnection?.IsLoggedIn == true)
            {
                GitHubTab.Initialize(gitHubConnection);
            }

            if (enterpriseConnection?.IsLoggedIn == true)
            {
                EnterpriseTab.Initialize(enterpriseConnection);
            }

            if (connection == gitHubConnection)
            {
                SelectedTabIndex = 0;
            }
            else if (connection == enterpriseConnection)
            {
                SelectedTabIndex = 1;
            }

            if (Url?.Host is string host && HostAddress.Create(host) is HostAddress hostAddress)
            {
                if (hostAddress == gitHubConnection?.HostAddress)
                {
                    GitHubTab.Filter = Url;
                    SelectedTabIndex = 0;
                }
                else if (hostAddress == enterpriseConnection?.HostAddress)
                {
                    EnterpriseTab.Filter = Url;
                    SelectedTabIndex = 1;
                }
            }

            this.WhenAnyValue(x => x.SelectedTabIndex).Subscribe(x => tabs[x].Activate().Forget());
        }

        async Task LoginAsDifferentUserAsync()
        {
            if (await dialogService.ShowLoginDialog() is IConnection connection)
            {
                var connections = await connectionManager.GetLoadedConnections();
                var gitHubConnection = connections.FirstOrDefault(x => x.HostAddress.IsGitHubDotCom());

                if (connection == gitHubConnection)
                {
                    SelectedTabIndex = 0;
                    GitHubTab.Initialize(connection);
                    GitHubTab.Activate().Forget();
                }
                else
                {
                    SelectedTabIndex = 1;
                    EnterpriseTab.Initialize(connection);
                    EnterpriseTab.Activate().Forget();
                }
            }
        }

        void BrowseForDirectory()
        {
            var result = os.Dialog.BrowseForDirectory(Path, Resources.BrowseForDirectory);

            if (result != BrowseDirectoryResult.Failed)
            {
                var path = result.DirectoryPath;
                var selected = tabs[SelectedTabIndex].Repository;

                if (selected != null)
                {
                    path = System.IO.Path.Combine(path, selected.Name);
                }

                Path = path;
            }
        }

        void UpdatePath(RepositoryModel repository)
        {
            if (repository != null)
            {
                var basePath = GetUpdatedBasePath(Path);
                previousRepository = repository;
                Path = System.IO.Path.Combine(basePath, repository.Owner, repository.Name);
            }
        }

        string GetUpdatedBasePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return service.DefaultClonePath;
            }

            if (previousRepository == null)
            {
                return path;
            }

            if (FindDirWithout(path, previousRepository?.Owner, 2) is string dirWithoutOwner)
            {
                return dirWithoutOwner;
            }

            if (FindDirWithout(path, previousRepository?.Name, 1) is string dirWithoutRepo)
            {
                return dirWithoutRepo;
            }

            return path;

            string FindDirWithout(string dir, string match, int levels)
            {
                string dirWithout = null;
                for (var i = 0; i < 2; i++)
                {
                    if (string.IsNullOrEmpty(dir))
                    {
                        break;
                    }

                    var name = System.IO.Path.GetFileName(dir);
                    dir = System.IO.Path.GetDirectoryName(dir);
                    if (name == match)
                    {
                        dirWithout = dir;
                    }
                }

                return dirWithout;
            }
        }

        string ValidatePathWarning(RepositoryModel repositoryModel, string path)
        {
            if (repositoryModel != null)
            {
                if (service.DestinationFileExists(path))
                {
                    return Resources.DestinationAlreadyExists;
                }

                if (service.DestinationDirectoryExists(path) && !service.DestinationDirectoryEmpty(path))
                {
                    using (var repository = gitService.GetRepository(path))
                    {
                        if (repository == null)
                        {
                            return Resources.DirectoryAtDestinationNotEmpty;
                        }

                        var localUrl = gitService.GetRemoteUri(repository)?.ToRepositoryUrl();
                        if (localUrl == null)
                        {
                            return Resources.LocalRepositoryDoesntHaveARemoteOrigin;
                        }

                        var targetUrl = repositoryModel.CloneUrl?.ToRepositoryUrl();
                        if (localUrl != targetUrl)
                        {
                            return string.Format(CultureInfo.CurrentCulture, Resources.LocalRepositoryHasARemoteOf,
                                localUrl);
                        }

                        return Resources.YouHaveAlreadyClonedToThisLocation;
                    }
                }
            }

            return null;
        }
    }
}
