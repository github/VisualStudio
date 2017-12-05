using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Info;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using ReactiveUI;
using OleMenuCommand = Microsoft.VisualStudio.Shell.OleMenuCommand;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// The view model for the GitHub Pane.
    /// </summary>
    [Export(typeof(IGitHubPaneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class GitHubPaneViewModel : ViewModelBase, IGitHubPaneViewModel, IDisposable
    {
        static readonly Regex pullUri = CreateRoute("/:owner/:repo/pull/:number");

        readonly IViewViewModelFactory viewModelFactory;
        readonly ISimpleApiClientFactory apiClientFactory;
        readonly IConnectionManager connectionManager;
        readonly ITeamExplorerServiceHolder teServiceHolder;
        readonly IVisualStudioBrowser browser;
        readonly IUsageTracker usageTracker;
        readonly INavigationViewModel navigator;
        readonly ILoggedOutViewModel loggedOut;
        readonly INotAGitHubRepositoryViewModel notAGitHubRepository;
        readonly INotAGitRepositoryViewModel notAGitRepository;
        readonly SemaphoreSlim navigating = new SemaphoreSlim(1);
        readonly ObservableAsPropertyHelper<ContentOverride> contentOverride;
        readonly ObservableAsPropertyHelper<bool> isSearchEnabled;
        readonly ObservableAsPropertyHelper<string> title;
        readonly ReactiveCommand<Unit> refresh;
        readonly ReactiveCommand<Unit> showPullRequests;
        readonly ReactiveCommand<object> openInBrowser;
        bool initialized;
        IViewModel content;
        ILocalRepositoryModel localRepository;
        string searchQuery;

        [ImportingConstructor]
        public GitHubPaneViewModel(
            IViewViewModelFactory viewModelFactory,
            ISimpleApiClientFactory apiClientFactory,
            IConnectionManager connectionManager,
            ITeamExplorerServiceHolder teServiceHolder,
            IVisualStudioBrowser browser,
            IUsageTracker usageTracker,
            INavigationViewModel navigator,
            ILoggedOutViewModel loggedOut,
            INotAGitHubRepositoryViewModel notAGitHubRepository,
            INotAGitRepositoryViewModel notAGitRepository)
        {
            Guard.ArgumentNotNull(viewModelFactory, nameof(viewModelFactory));
            Guard.ArgumentNotNull(apiClientFactory, nameof(apiClientFactory));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(teServiceHolder, nameof(teServiceHolder));
            Guard.ArgumentNotNull(browser, nameof(browser));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));
            Guard.ArgumentNotNull(navigator, nameof(navigator));
            Guard.ArgumentNotNull(loggedOut, nameof(loggedOut));
            Guard.ArgumentNotNull(notAGitHubRepository, nameof(notAGitHubRepository));
            Guard.ArgumentNotNull(notAGitRepository, nameof(notAGitRepository));

            this.viewModelFactory = viewModelFactory;
            this.apiClientFactory = apiClientFactory;
            this.connectionManager = connectionManager;
            this.teServiceHolder = teServiceHolder;
            this.browser = browser;
            this.usageTracker = usageTracker;
            this.navigator = navigator;
            this.loggedOut = loggedOut;
            this.notAGitHubRepository = notAGitHubRepository;
            this.notAGitRepository = notAGitRepository;

            var contentAndNavigatorContent = Observable.CombineLatest(
                this.WhenAnyValue(x => x.Content),
                navigator.WhenAnyValue(x => x.Content),
                (c, nc) => new { Content = c, NavigatorContent = nc });

            contentOverride = contentAndNavigatorContent
                .SelectMany(x =>
                {
                    if (x.Content == null) return Observable.Return(ContentOverride.Spinner);
                    else if (x.Content == navigator && x.NavigatorContent != null)
                    {
                        return x.NavigatorContent.WhenAnyValue(
                            y => y.IsLoading,
                            y => y.Error,
                            (l, e) =>
                            {
                                if (l) return ContentOverride.Spinner;
                                if (e != null) return ContentOverride.Error;
                                else return ContentOverride.None;
                            });
                    }
                    else return Observable.Return(ContentOverride.None);
                })
                .ToProperty(this, x => x.ContentOverride);

            // Returns navigator.Content if Content == navigator, otherwise null.
            var currentPage = contentAndNavigatorContent
                .Select(x => x.Content == navigator ? x.NavigatorContent : null);

            title = currentPage
                .SelectMany(x => x?.WhenAnyValue(y => y.Title) ?? Observable.Return<string>(null))
                .Select(x => x ?? "GitHub")
                .ToProperty(this, x => x.Title);

            isSearchEnabled = currentPage
                .Select(x => x is ISearchablePageViewModel)
                .ToProperty(this, x => x.IsSearchEnabled);

            refresh = ReactiveCommand.CreateAsyncTask(
                currentPage.SelectMany(x => x?.WhenAnyValue(
                        y => y.IsLoading,
                        y => y.IsBusy,
                        (loading, busy) => !loading && !busy)
                            ?? Observable.Return(false)),
                _ => navigator.Content.Refresh());
            refresh.ThrownExceptions.Subscribe();

            showPullRequests = ReactiveCommand.CreateAsyncTask(
                this.WhenAny(x => x.Content, x => x.Value == navigator),
                _ => ShowPullRequests());

            openInBrowser = ReactiveCommand.Create(currentPage.Select(x => x is IOpenInBrowser));
            openInBrowser.Subscribe(_ =>
            {
                var url = ((IOpenInBrowser)navigator.Content).WebUrl;
                if (url != null) browser.OpenUrl(url);
            });

            navigator.WhenAnyObservable(x => x.Content.NavigationRequested)
                .Subscribe(x => NavigateTo(x).Forget());

            this.WhenAnyValue(x => x.SearchQuery)
                .Where(x => navigator.Content is ISearchablePageViewModel)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => ((ISearchablePageViewModel)navigator.Content).SearchQuery = x);
        }

        /// <inheritdoc/>
        public IConnection Connection
        {
            get;
            private set;
        }

        /// <inheritdoc/>
        public IViewModel Content
        {
            get { return content; }
            private set { this.RaiseAndSetIfChanged(ref content, value); }
        }

        /// <inheritdoc/>
        public ContentOverride ContentOverride => contentOverride.Value;

        /// <inheritdoc/>
        public bool IsSearchEnabled => isSearchEnabled.Value;

        /// <inheritdoc/>
        public ILocalRepositoryModel LocalRepository
        {
            get { return localRepository; }
            private set { this.RaiseAndSetIfChanged(ref localRepository, value); }
        }

        /// <inheritdoc/>
        public string SearchQuery
        {
            get { return searchQuery; }
            set { this.RaiseAndSetIfChanged(ref searchQuery, value); }
        }

        /// <inheritdoc/>
        public string Title => title.Value;

        /// <inheritdoc/>
        public void Dispose()
        {
            navigating.Dispose();
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(IServiceProvider paneServiceProvider)
        {
            await UpdateContent(teServiceHolder.ActiveRepo);
            teServiceHolder.Subscribe(this, x => UpdateContent(x).Forget());
            connectionManager.Connections.CollectionChanged += (_, __) => UpdateContent(LocalRepository).Forget();

            BindNavigatorCommand(paneServiceProvider, PkgCmdIDList.pullRequestCommand, showPullRequests);
            BindNavigatorCommand(paneServiceProvider, PkgCmdIDList.backCommand, navigator.NavigateBack);
            BindNavigatorCommand(paneServiceProvider, PkgCmdIDList.forwardCommand, navigator.NavigateForward);
            BindNavigatorCommand(paneServiceProvider, PkgCmdIDList.refreshCommand, refresh);
            BindNavigatorCommand(paneServiceProvider, PkgCmdIDList.githubCommand, openInBrowser);

            paneServiceProvider.AddCommandHandler(Guids.guidGitHubToolbarCmdSet, PkgCmdIDList.helpCommand,
                 (_, __) =>
                 {
                     browser.OpenUrl(new Uri(GitHubUrls.Documentation));
                     usageTracker.IncrementCounter(x => x.NumberOfGitHubPaneHelpClicks).Forget();
                 });
        }

        /// <inheritdoc/>
        public async Task NavigateTo(Uri uri)
        {
            Guard.ArgumentNotNull(uri, nameof(uri));

            if (uri.Scheme != "github")
            {
                throw new NotSupportedException("Invalid URI scheme for GitHub pane: " + uri.Scheme);
            }

            if (uri.Authority != "pane")
            {
                throw new NotSupportedException("Invalid URI authority for GitHub pane: " + uri.Authority);
            }

            Match match;

            if (uri.AbsolutePath == "/pulls")
            {
                await ShowPullRequests();
            }
            else if (uri.AbsolutePath == "/pull/new")
            {
                await ShowCreatePullRequest();
            }
            else if ((match = pullUri.Match(uri.AbsolutePath))?.Success == true)
            {
                var owner = match.Groups["owner"].Value;
                var repo = match.Groups["repo"].Value;
                var number = int.Parse(match.Groups["number"].Value);
                await ShowPullRequest(owner, repo, number);
            }
            else
            {
                throw new NotSupportedException("Unrecognised GitHub pane URL: " + uri.AbsolutePath);
            }

            var queries = HttpUtility.ParseQueryString(uri.Query);

            if (queries["refresh"] == "true")
            {
                await navigator.Content.Refresh();
            }
        }

        /// <inheritdoc/>
        public Task ShowDefaultPage() => ShowPullRequests();

        /// <inheritdoc/>
        public Task ShowCreatePullRequest()
        {
            return NavigateTo<IPullRequestCreationViewModel>(x => x.InitializeAsync(LocalRepository, Connection));
        }

        /// <inheritdoc/>
        public Task ShowPullRequests()
        {
            return NavigateTo<IPullRequestListViewModel>(x => x.InitializeAsync(LocalRepository, Connection)); 
        }

        /// <inheritdoc/>
        public Task ShowPullRequest(string owner, string repo, int number)
        {
            Guard.ArgumentNotNull(owner, nameof(owner));
            Guard.ArgumentNotNull(repo, nameof(repo));

            return NavigateTo<IPullRequestDetailViewModel>(
                x => x.InitializeAsync(LocalRepository, Connection, owner, repo, number),
                x => x.RemoteRepositoryOwner == owner && x.LocalRepository.Name == repo && x.Number == number);
        }

        OleMenuCommand BindNavigatorCommand<T>(IServiceProvider paneServiceProvider, int commandId, ReactiveCommand<T> command)
        {
            Guard.ArgumentNotNull(paneServiceProvider, nameof(paneServiceProvider));
            Guard.ArgumentNotNull(command, nameof(command));

            Func<bool> canExecute = () => Content == navigator && command.CanExecute(null);

            var result = paneServiceProvider.AddCommandHandler(
                Guids.guidGitHubToolbarCmdSet,
                commandId,
                canExecute,
                () => command.Execute(null),
                true);

            Observable.CombineLatest(
                this.WhenAnyValue(x => x.Content),
                command.CanExecuteObservable,
                (c, e) => c == navigator && e)
                .Subscribe(x => result.Enabled = x);

            return result;
        }

        async Task NavigateTo<TViewModel>(Func<TViewModel, Task> initialize, Func<TViewModel, bool> match = null)
            where TViewModel : class, IPanePageViewModel
        {
            Guard.ArgumentNotNull(initialize, nameof(initialize));

            if (Content != navigator) return;
            await navigating.WaitAsync();

            try
            {
                var viewModel = navigator.History
                    .OfType<TViewModel>()
                    .FirstOrDefault(x => match?.Invoke(x) ?? true);

                if (viewModel == null)
                {
                    viewModel = viewModelFactory.CreateViewModel<TViewModel>();
                    navigator.NavigateTo(viewModel);
                    await initialize(viewModel);
                }
                else if (navigator.Content != viewModel)
                {
                    navigator.NavigateTo(viewModel);
                }
            }
            finally
            {
                navigating.Release();
            }
        }

        async Task UpdateContent(ILocalRepositoryModel repository)
        {
            if (initialized && Equals(repository, LocalRepository)) return;

            initialized = true;
            LocalRepository = repository;
            Connection = null;
            Content = null;
            navigator.Clear();

            if (repository == null)
            {
                Content = notAGitRepository;
                return;
            }
            else if (string.IsNullOrWhiteSpace(repository.CloneUrl))
            {
                Content = notAGitHubRepository;
                return;
            }

            var repositoryUrl = repository.CloneUrl.ToRepositoryUrl();
            var isDotCom = HostAddress.IsGitHubDotComUri(repositoryUrl);
            var client = await apiClientFactory.Create(repository.CloneUrl);
            var isEnterprise = isDotCom ? false : client.IsEnterprise();

            if ((isDotCom || isEnterprise) && await IsValidRepository(client))
            {
                var hostAddress = HostAddress.Create(repository.CloneUrl);

                Connection = await connectionManager.GetConnection(hostAddress);

                if (Connection != null)
                {
                    Content = navigator;
                    await ShowDefaultPage();
                }
                else
                {
                    Content = loggedOut;
                }
            }
            else
            {
                Content = notAGitHubRepository;
            }
        }

        static async Task<bool> IsValidRepository(ISimpleApiClient client)
        {
            try
            {
                var repo = await client.GetRepository();
                return repo.Id != 0;
            }
            catch
            {
                return false;
            }
        }

        static Regex CreateRoute(string route)
        {
            // Build RegEx from route (:foo to named group (?<foo>[a-z0-9]+)).
            var routeFormat = new Regex("(:([a-z]+))\\b").Replace(route, "(?<$2>[a-z0-9]+)");
            return new Regex(routeFormat, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        }
    }
}
