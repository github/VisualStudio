using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Globalization;
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
using GitHub.Logging;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.Services.Vssdk.Commands;
using GitHub.VisualStudio;
using ReactiveUI;
using Serilog;
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
        static readonly ILogger log = LogManager.ForContext<GitHubPaneViewModel>();
        static readonly Regex pullUri = CreateRoute("/:owner/:repo/pull/:number");
        static readonly Regex pullNewReviewUri = CreateRoute("/:owner/:repo/pull/:number/review/new");
        static readonly Regex pullUserReviewsUri = CreateRoute("/:owner/:repo/pull/:number/reviews/:login");
        static readonly Regex pullCheckRunsUri = CreateRoute("/:owner/:repo/pull/:number/checkruns/:id");

        readonly IViewViewModelFactory viewModelFactory;
        readonly ISimpleApiClientFactory apiClientFactory;
        readonly IConnectionManager connectionManager;
        readonly ITeamExplorerContext teamExplorerContext;
        readonly INavigationViewModel navigator;
        readonly ILoggedOutViewModel loggedOut;
        readonly INotAGitHubRepositoryViewModel notAGitHubRepository;
        readonly INotAGitRepositoryViewModel notAGitRepository;
        readonly INoRemoteOriginViewModel noRemoteOrigin;
        readonly ILoginFailedViewModel loginFailed;
        readonly SemaphoreSlim navigating = new SemaphoreSlim(1);
        readonly ObservableAsPropertyHelper<ContentOverride> contentOverride;
        readonly ObservableAsPropertyHelper<bool> isSearchEnabled;
        readonly ObservableAsPropertyHelper<string> title;
        readonly ReactiveCommand<Unit, Unit> refresh;
        readonly ReactiveCommand<Unit, Unit> showPullRequests;
        readonly ReactiveCommand<Unit, Unit> openInBrowser;
        readonly ReactiveCommand<Unit, Unit> help;
        IDisposable connectionSubscription;
        Task initializeTask;
        IViewModel content;
        LocalRepositoryModel localRepository;
        string searchQuery;

        [ImportingConstructor]
        public GitHubPaneViewModel(
            IViewViewModelFactory viewModelFactory,
            ISimpleApiClientFactory apiClientFactory,
            IConnectionManager connectionManager,
            ITeamExplorerContext teamExplorerContext,
            IVisualStudioBrowser browser,
            IUsageTracker usageTracker,
            INavigationViewModel navigator,
            ILoggedOutViewModel loggedOut,
            INotAGitHubRepositoryViewModel notAGitHubRepository,
            INotAGitRepositoryViewModel notAGitRepository,
            INoRemoteOriginViewModel noRemoteOrigin,
            ILoginFailedViewModel loginFailed)
        {
            Guard.ArgumentNotNull(viewModelFactory, nameof(viewModelFactory));
            Guard.ArgumentNotNull(apiClientFactory, nameof(apiClientFactory));
            Guard.ArgumentNotNull(connectionManager, nameof(connectionManager));
            Guard.ArgumentNotNull(teamExplorerContext, nameof(teamExplorerContext));
            Guard.ArgumentNotNull(browser, nameof(browser));
            Guard.ArgumentNotNull(usageTracker, nameof(usageTracker));
            Guard.ArgumentNotNull(navigator, nameof(navigator));
            Guard.ArgumentNotNull(loggedOut, nameof(loggedOut));
            Guard.ArgumentNotNull(notAGitHubRepository, nameof(notAGitHubRepository));
            Guard.ArgumentNotNull(notAGitRepository, nameof(notAGitRepository));
            Guard.ArgumentNotNull(noRemoteOrigin, nameof(noRemoteOrigin));
            Guard.ArgumentNotNull(loginFailed, nameof(loginFailed));

            this.viewModelFactory = viewModelFactory;
            this.apiClientFactory = apiClientFactory;
            this.connectionManager = connectionManager;
            this.teamExplorerContext = teamExplorerContext;
            this.navigator = navigator;
            this.loggedOut = loggedOut;
            this.notAGitHubRepository = notAGitHubRepository;
            this.notAGitRepository = notAGitRepository;
            this.noRemoteOrigin = noRemoteOrigin;
            this.loginFailed = loginFailed;

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

            refresh = ReactiveCommand.CreateFromTask(
                () => navigator.Content.Refresh(),
                currentPage.SelectMany(x => x?.WhenAnyValue(
                        y => y.IsLoading,
                        y => y.IsBusy,
                        (loading, busy) => !loading && !busy)
                            ?? Observable.Return(false)));
            refresh.ThrownExceptions.Subscribe();

            showPullRequests = ReactiveCommand.CreateFromTask(
                ShowPullRequests,
                this.WhenAny(x => x.Content, x => x.Value == navigator));

            openInBrowser = ReactiveCommand.Create(
                () =>
                {
                    var url = ((IOpenInBrowser)navigator.Content).WebUrl;
                    if (url != null) browser.OpenUrl(url);
                },
                currentPage.Select(x => x is IOpenInBrowser));

            help = ReactiveCommand.Create(() => { });
            help.Subscribe(_ =>
            {
                browser.OpenUrl(new Uri(GitHubUrls.Documentation));
                usageTracker.IncrementCounter(x => x.NumberOfGitHubPaneHelpClicks).Forget();
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
        public LocalRepositoryModel LocalRepository
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
        public Task InitializeAsync(IServiceProvider paneServiceProvider)
        {
            return initializeTask = initializeTask ?? CreateInitializeTask(paneServiceProvider);
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
                var number = int.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);
                await ShowPullRequest(owner, repo, number);
            }
            else if ((match = pullNewReviewUri.Match(uri.AbsolutePath))?.Success == true)
            {
                var owner = match.Groups["owner"].Value;
                var repo = match.Groups["repo"].Value;
                var number = int.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);
                await ShowPullRequestReviewAuthoring(owner, repo, number);
            }
            else if ((match = pullUserReviewsUri.Match(uri.AbsolutePath))?.Success == true)
            {
                var owner = match.Groups["owner"].Value;
                var repo = match.Groups["repo"].Value;
                var number = int.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);
                var login = match.Groups["login"].Value;
                await ShowPullRequestReviews(owner, repo, number, login);
            }
            else if ((match = pullCheckRunsUri.Match(uri.AbsolutePath))?.Success == true)
            {
                var owner = match.Groups["owner"].Value;
                var repo = match.Groups["repo"].Value;
                var number = int.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture);
                var id = match.Groups["id"].Value;

                await ShowPullRequestCheckRun(owner, repo, number, id);
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

        /// <inheritdoc/>
        public Task ShowPullRequestReviews(string owner, string repo, int number, string login)
        {
            Guard.ArgumentNotNull(owner, nameof(owner));
            Guard.ArgumentNotNull(repo, nameof(repo));

            return NavigateTo<IPullRequestUserReviewsViewModel>(
                x => x.InitializeAsync(LocalRepository, Connection, owner, repo, number, login),
                x => x.RemoteRepositoryOwner == owner &&
                     x.LocalRepository.Name == repo &&
                     x.PullRequestNumber == number &&
                     x.User.Login == login);
        }

        /// <inheritdoc/>
        public Task ShowPullRequestCheckRun(string owner, string repo, int number, string checkRunId)
        {
            Guard.ArgumentNotNull(owner, nameof(owner));
            Guard.ArgumentNotNull(repo, nameof(repo));

            return NavigateTo<IPullRequestAnnotationsViewModel>(
                x => x.InitializeAsync(LocalRepository, Connection, owner, repo, number, checkRunId),
                x => x.RemoteRepositoryOwner == owner &&
                     x.LocalRepository.Name == repo &&
                     x.PullRequestNumber == number &&
                     x.CheckRunId == checkRunId);
        }

        /// <inheritdoc/>
        public Task ShowPullRequestReviewAuthoring(string owner, string repo, int number)
        {
            Guard.ArgumentNotNull(owner, nameof(owner));
            Guard.ArgumentNotNull(repo, nameof(repo));

            return NavigateTo<IPullRequestReviewAuthoringViewModel>(
                x => x.InitializeAsync(LocalRepository, Connection, owner, repo, number),
                x => x.RemoteRepositoryOwner == owner &&
                     x.LocalRepository.Name == repo &&
                     x.PullRequestModel.Number == number);
        }

        async Task CreateInitializeTask(IServiceProvider paneServiceProvider)
        {
            await UpdateContent(teamExplorerContext.ActiveRepository);
            teamExplorerContext.WhenAnyValue(x => x.ActiveRepository)
               .Skip(1)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Subscribe(x => UpdateContent(x).Forget(log));

            connectionManager.Connections.CollectionChanged += (_, __) => UpdateContent(LocalRepository).Forget(log);

            var menuService = (IMenuCommandService)paneServiceProvider.GetService(typeof(IMenuCommandService));
            BindNavigatorCommand(menuService, PkgCmdIDList.pullRequestCommand, showPullRequests);
            BindNavigatorCommand(menuService, PkgCmdIDList.backCommand, navigator.NavigateBack);
            BindNavigatorCommand(menuService, PkgCmdIDList.forwardCommand, navigator.NavigateForward);
            BindNavigatorCommand(menuService, PkgCmdIDList.refreshCommand, refresh);
            BindNavigatorCommand(menuService, PkgCmdIDList.githubCommand, openInBrowser);
            BindNavigatorCommand(menuService, PkgCmdIDList.helpCommand, help);
        }

        OleMenuCommand BindNavigatorCommand<P, R>(IMenuCommandService menu, int commandId, ReactiveCommand<P, R> command)
        {
            Guard.ArgumentNotNull(menu, nameof(menu));
            Guard.ArgumentNotNull(command, nameof(command));

            return menu.BindCommand(new CommandID(Guids.guidGitHubToolbarCmdSet, commandId), command);
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

        async Task UpdateContent(LocalRepositoryModel repository)
        {
            log.Debug("UpdateContent called with {CloneUrl}", repository?.CloneUrl);

            LocalRepository = repository;
            connectionSubscription?.Dispose();
            connectionSubscription = null;
            Connection = null;
            Content = null;
            navigator.Clear();

            if (repository == null)
            {
                log.Debug("Not a git repository: {CloneUrl}", repository?.CloneUrl);
                Content = notAGitRepository;
                return;
            }
            else if (string.IsNullOrWhiteSpace(repository.CloneUrl))
            {
                if (repository.HasRemotesButNoOrigin)
                {
                    log.Debug("No origin remote");
                    Content = noRemoteOrigin;
                }
                else
                {
                    log.Debug("Not a GitHub repository: {CloneUrl}", repository?.CloneUrl);
                    Content = notAGitHubRepository;
                }

                return;
            }

            var repositoryUrl = repository.CloneUrl.ToRepositoryUrl();
            var isDotCom = HostAddress.IsGitHubDotComUri(repositoryUrl);
            var client = await apiClientFactory.Create(repository.CloneUrl);
            var isEnterprise = isDotCom ? false : await client.IsEnterprise();
            var notGitHubRepo = true;

            if (isDotCom || isEnterprise)
            {
                var hostAddress = HostAddress.Create(repository.CloneUrl);

                notGitHubRepo = false;

                Connection = await connectionManager.GetConnection(hostAddress);
                Connection?.WhenAnyValue(
                    x => x.IsLoggedIn,
                    x => x.IsLoggingIn,
                    (_, __) => Unit.Default)
                    .Skip(1)
                    .Throttle(TimeSpan.FromMilliseconds(100))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => UpdateContent(LocalRepository).Forget());

                if (Connection?.IsLoggedIn == true)
                {
                    if (await IsValidRepository(client) == true)
                    {
                        log.Debug("Found a GitHub repository: {CloneUrl}", repository?.CloneUrl);
                        Content = navigator;
                        await ShowDefaultPage();
                    }
                    else
                    {
                        notGitHubRepo = true;
                    }
                }
                else if (Connection?.IsLoggingIn == true)
                {
                    log.Debug("Found a GitHub repository: {CloneUrl} and logging in", repository?.CloneUrl);
                    Content = null;
                }
                else if (Connection?.ConnectionError != null)
                {
                    log.Debug("Found a GitHub repository: {CloneUrl} with login error", repository?.CloneUrl);
                    loginFailed.Initialize(Connection.ConnectionError.GetUserFriendlyError(ErrorType.LoginFailed));
                    Content = loginFailed;
                }
                else
                {
                    log.Debug("Found a a GitHub repository but not logged in: {CloneUrl}", repository?.CloneUrl);
                    Content = loggedOut;
                }
            }

            if (notGitHubRepo)
            {
                log.Debug("Not a GitHub repository: {CloneUrl}", repository?.CloneUrl);
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
            // Build RegEx from route (:foo to named group (?<foo>[\w_.\-=]+)).
            var routeFormat = "^" + new Regex("(:([a-z]+))\\b").Replace(route, @"(?<$2>[\w_.\-=]+)") + "$";
            return new Regex(routeFormat, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        }
    }
}
