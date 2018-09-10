using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.App;
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
        readonly IReadOnlyList<IRepositoryCloneTabViewModel> tabs;
        string path;
        string previousOwner;
        ObservableAsPropertyHelper<string> pathError;
        int selectedTabIndex;

        [ImportingConstructor]
        public RepositoryCloneViewModel(
            IOperatingSystem os,
            IConnectionManager connectionManager,
            IRepositoryCloneService service,
            IRepositorySelectViewModel gitHubTab,
            IRepositorySelectViewModel enterpriseTab,
            IRepositoryUrlViewModel urlTab)
        {
            this.os = os;
            this.connectionManager = connectionManager;
            this.service = service;

            GitHubTab = gitHubTab;
            EnterpriseTab = enterpriseTab;
            UrlTab = urlTab;
            tabs = new IRepositoryCloneTabViewModel[] { GitHubTab, EnterpriseTab, UrlTab };

            var repository = this.WhenAnyValue(x => x.SelectedTabIndex)
                .SelectMany(x => tabs[x].WhenAnyValue(tab => tab.Repository));

            Path = service.DefaultClonePath;
            repository.Subscribe(x => UpdatePath(x));

            pathError = Observable.CombineLatest(
                repository,
                this.WhenAnyValue(x => x.Path),
                ValidatePath)
                .ToProperty(this, x => x.PathError);

            var canClone = Observable.CombineLatest(
                repository,
                this.WhenAnyValue(x => x.PathError),
                (repo, error) => (repo, error))
                .Select(x => x.repo != null && x.error == null);

            Browse = ReactiveCommand.Create().OnExecuteCompleted(_ => BrowseForDirectory());
            Clone = ReactiveCommand.CreateAsyncObservable(
                canClone,
                _ => repository.Select(x => new CloneDialogResult(Path, x)));
        }

        public IRepositorySelectViewModel GitHubTab { get; }
        public IRepositorySelectViewModel EnterpriseTab { get; }
        public IRepositoryUrlViewModel UrlTab { get; }

        public string Path
        {
            get => path;
            set => this.RaiseAndSetIfChanged(ref path, value);
        }

        public string PathError => pathError.Value;

        public int SelectedTabIndex
        {
            get => selectedTabIndex;
            set => this.RaiseAndSetIfChanged(ref selectedTabIndex, value);
        }

        public string Title => Resources.CloneTitle;

        public IObservable<object> Done => Clone;

        public ReactiveCommand<object> Browse { get; }

        public ReactiveCommand<CloneDialogResult> Clone { get; }

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

        void UpdatePath(IRepositoryModel repository)
        {
            if (repository != null)
            {
                var basePath = GetBasePath(Path, previousOwner);
                previousOwner = repository.Owner;
                Path = System.IO.Path.Combine(basePath, repository.Owner, repository.Name);
            }
        }

        static string GetBasePath(string path, string owner)
        {
            if (owner != null)
            {
                var dir = path;
                for (var i = 0; i < 2; i++)
                {
                    if (string.IsNullOrEmpty(dir))
                    {
                        break;
                    }

                    var name = System.IO.Path.GetFileName(dir);
                    dir = System.IO.Path.GetDirectoryName(dir);
                    if (name == owner)
                    {
                        return dir;
                    }
                }
            }

            return path;
        }

        string ValidatePath(IRepositoryModel repository, string path)
        {
            if (repository != null)
            {
                return service.DestinationExists(path) ?
                    Resources.DestinationAlreadyExists :
                    null;
            }

            return null;
        }
    }
}
