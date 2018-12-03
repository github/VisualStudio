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
        static readonly ILogger log = LogManager.ForContext<RepositoryCloneViewModel>();
        readonly IOperatingSystem os;
        readonly IConnectionManager connectionManager;
        readonly IRepositoryCloneService service;
        readonly IGitService gitService;
        readonly IUsageService usageService;
        readonly IUsageTracker usageTracker;
        readonly IReadOnlyList<IRepositoryCloneTabViewModel> tabs;
        string path;
        RepositoryModel previousRepository;
        ObservableAsPropertyHelper<string> pathWarning;
        int selectedTabIndex;

        [ImportingConstructor]
        public RepositoryCloneViewModel(
            IOperatingSystem os,
            IConnectionManager connectionManager,
            IRepositoryCloneService service,
            IGitService gitService,
            IUsageService usageService,
            IUsageTracker usageTracker,
            IRepositorySelectViewModel gitHubTab,
            IRepositorySelectViewModel enterpriseTab,
            IRepositoryUrlViewModel urlTab)
        {
            this.os = os;
            this.connectionManager = connectionManager;
            this.service = service;
            this.gitService = gitService;
            this.usageService = usageService;
            this.usageTracker = usageTracker;

            GitHubTab = gitHubTab;
            EnterpriseTab = enterpriseTab;
            UrlTab = urlTab;
            tabs = new IRepositoryCloneTabViewModel[] { GitHubTab, EnterpriseTab, UrlTab };

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
                (repo, path) => repo != null && !service.DestinationFileExists(path) && !service.DestinationDirectoryExists(path));

            var canOpen = Observable.CombineLatest(
                repository, this.WhenAnyValue(x => x.Path),
                (repo, path) => repo != null && !service.DestinationFileExists(path) && service.DestinationDirectoryExists(path));

            Browse = ReactiveCommand.Create(() => BrowseForDirectory());
            Clone = ReactiveCommand.CreateFromObservable(
                () => repository.Select(x => new CloneDialogResult(Path, x?.CloneUrl)),
                canClone);
            Open = ReactiveCommand.CreateFromObservable(
                () => repository.Select(x => new CloneDialogResult(Path, x?.CloneUrl)),
                canOpen);
        }

        public IRepositorySelectViewModel GitHubTab { get; }
        public IRepositorySelectViewModel EnterpriseTab { get; }
        public IRepositoryUrlViewModel UrlTab { get; }

        public string Path
        {
            get => path;
            set => this.RaiseAndSetIfChanged(ref path, value);
        }

        public string PathWarning => pathWarning.Value;

        public int SelectedTabIndex
        {
            get => selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref selectedTabIndex, value);
        }

        public string Title => Resources.OpenFromGitHubTitle;

        public IObservable<object> Done => Observable.Merge(Clone, Open);

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

            if (connection == enterpriseConnection)
            {
                SelectedTabIndex = 1;
            }

            this.WhenAnyValue(x => x.SelectedTabIndex).Subscribe(x => tabs[x].Activate().Forget());

            // When a clipboard URL has been set or a user is in group A, show the URL tab by default
            if (!string.IsNullOrEmpty(UrlTab.Url) || await IsGroupA().ConfigureAwait(false))
            {
                SelectedTabIndex = 2;
            }

            switch (SelectedTabIndex)
            {
                case 0:
                    usageTracker.IncrementCounter(model => model.NumberOfCloneViewGitHubTab).Forget();
                    break;
                case 1:
                    usageTracker.IncrementCounter(model => model.NumberOfCloneViewEnterpriseTab).Forget();
                    break;
                case 2:
                    usageTracker.IncrementCounter(model => model.NumberOfCloneViewUrlTab).Forget();
                    break;
            }
        }

        // Put 50% of users in group A
        async Task<bool> IsGroupA()
        {
            var userGuid = await usageService.GetUserGuid().ConfigureAwait(false);
            var lastByte = userGuid.ToByteArray().Last();
            return lastByte % 2 == 0;
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

                if (service.DestinationDirectoryExists(path))
                {
                    using (var repository = gitService.GetRepository(path))
                    {
                        if (repository == null)
                        {
                            return Resources.CantFindARepositoryAtLocalPath;
                        }

                        var localUrl = gitService.GetRemoteUri(repository)?.ToRepositoryUrl();
                        if (localUrl == null)
                        {
                            return Resources.LocalRepositoryDoesntHaveARemoteOrigin;
                        }

                        var targetUrl = repositoryModel.CloneUrl?.ToRepositoryUrl();
                        if (localUrl != targetUrl)
                        {
                            return string.Format(CultureInfo.CurrentCulture, Resources.LocalRepositoryHasARemoteOf, localUrl);
                        }

                        return Resources.YouHaveAlreadyClonedToThisLocation;
                    }
                }
            }

            return null;
        }
    }
}
