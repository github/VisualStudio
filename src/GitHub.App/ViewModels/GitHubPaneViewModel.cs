using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// View model for the GitHub pane.
    /// </summary>
    [Export(typeof(GitHubPaneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [NullGuard(ValidationFlags.None)]
    public class GitHubPaneViewModel : ReactiveObject, IDisposable
    {
        readonly IServiceProvider serviceProvider;
        readonly ITeamExplorerServiceHolder holder;
        readonly ISimpleApiClientFactory apiFactory;
        readonly IConnectionManager connectionManager;
        readonly IRepositoryHosts hosts;
        readonly ObservableAsPropertyHelper<ReactiveCommand<object>> refresh;
        ISimpleRepositoryModel activeRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubPaneViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public GitHubPaneViewModel(
            [Import(typeof(Microsoft.VisualStudio.Shell.SVsServiceProvider))] IServiceProvider serviceProvider,
            ITeamExplorerServiceHolder holder,
            ISimpleApiClientFactory apiFactory,
            IConnectionManager connectionManager,
            IRepositoryHosts hosts,
            INavigationViewModel<IGitHubPanePage> navigator)
        {
            this.serviceProvider = serviceProvider;
            this.holder = holder;
            this.apiFactory = apiFactory;
            this.connectionManager = connectionManager;
            this.hosts = hosts;
            this.Navigation = navigator;
            holder.Subscribe(this, x => ActiveRepo = x);

            refresh = this.WhenAnyValue(x => x.Navigation.Content.Refresh).ToProperty(this, x => x.Refresh);
            this.WhenAnyValue(x => x.ActiveRepo).Subscribe(RepositoryChanged);
        }

        /// <summary>
        /// Gets the active repository.
        /// </summary>
        public ISimpleRepositoryModel ActiveRepo
        {
            get { return activeRepo; }
            private set { this.RaiseAndSetIfChanged(ref activeRepo, value); }
        }

        /// <summary>
        /// Gets an error message to display.
        /// </summary>
        public string ErrorMessage => string.Empty;

        /// <summary>
        /// Gets the navigator.
        /// </summary>
        public INavigationViewModel<IGitHubPanePage> Navigation { get; }

        /// <summary>
        /// Gets a title to display at the top of the pane.
        /// </summary>
        public string Title => "GitHub";

        /// <summary>
        /// Gets a command used to refresh the current page.
        /// </summary>
        public ReactiveCommand<object> Refresh => refresh.Value;

        /// <summary>
        /// Disposes any resources held by the view model.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            holder.Unsubscribe(this);
        }

        /// <summary>
        /// Called when <see cref="ActiveRepo"/> changes.
        /// </summary>
        /// <param name="repo">The new active repo.</param>
        async void RepositoryChanged(ISimpleRepositoryModel repo)
        {
            Navigation.Clear();

            var origin = RepositoryOrigin.Unknown;
            var page = default(IGitHubPanePage);

            if (repo != null)
            {
                var apiClient = apiFactory.Create(repo.CloneUrl);
                origin = await repo.GetOrigin(apiClient);

                // HACK
                var connection = await connectionManager.LookupConnection(repo);
                var isLoggedIn = await connection.IsLoggedIn(hosts);
                
                if (!isLoggedIn) return;

                var uiController = serviceProvider.GetExportedValue<IUIController>();
                uiController.Start(connection);
            }

            switch (origin)
            {
                case RepositoryOrigin.DotCom:
                case RepositoryOrigin.Enterprise:
                    var prList = serviceProvider.GetExportedValue<IPullRequestListViewModel>();
                    prList.Initialize(null);
                    page = prList;
                    break;
                case RepositoryOrigin.Other:
                    page = serviceProvider.GetExportedValue<INotAGitHubRepositoryViewModel>();
                    break;
                default:
                    page = serviceProvider.GetExportedValue<INotAGitRepositoryViewModel>();
                    break;
            }

            Navigation.NavigateTo(page);
        }
    }
}
