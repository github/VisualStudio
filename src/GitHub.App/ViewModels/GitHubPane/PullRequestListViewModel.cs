using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GitHub.App;
using GitHub.Collections;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using ReactiveUI;
using Serilog;
using static System.FormattableString;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IPullRequestListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestListViewModel : PanePageViewModelBase, IPullRequestListViewModel
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestListViewModel>();

        readonly IModelServiceFactory modelServiceFactory;
        readonly TrackingCollection<IAccount> trackingAuthors;
        readonly TrackingCollection<IAccount> trackingAssignees;
        readonly IPackageSettings settings;
        readonly IVisualStudioBrowser visualStudioBrowser;
        readonly bool constructing;
        PullRequestListUIState listSettings;
        ILocalRepositoryModel localRepository;
        IRemoteRepositoryModel remoteRepository;
        IModelService modelService;

        [ImportingConstructor]
        public PullRequestListViewModel(
            IModelServiceFactory modelServiceFactory,
            IPackageSettings settings,
            IPullRequestSessionManager sessionManager,
            IVisualStudioBrowser visualStudioBrowser)
        {
            Guard.ArgumentNotNull(modelServiceFactory, nameof(modelServiceFactory));
            Guard.ArgumentNotNull(settings, nameof(settings));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(visualStudioBrowser, nameof(visualStudioBrowser));

            constructing = true;
            this.modelServiceFactory = modelServiceFactory;
            this.settings = settings;
            this.visualStudioBrowser = visualStudioBrowser;

            Title = Resources.PullRequestsNavigationItemText;

            States = new List<PullRequestState> {
                new PullRequestState { IsOpen = true, Name = "Open" },
                new PullRequestState { IsOpen = false, Name = "Closed" },
                new PullRequestState { Name = "All" }
            };

            trackingAuthors = new TrackingCollection<IAccount>(Observable.Empty<IAccount>(),
                OrderedComparer<IAccount>.OrderByDescending(x => x.Login).Compare);
            trackingAssignees = new TrackingCollection<IAccount>(Observable.Empty<IAccount>(),
                OrderedComparer<IAccount>.OrderByDescending(x => x.Login).Compare);
            trackingAuthors.Subscribe();
            trackingAssignees.Subscribe();

            Authors = trackingAuthors.CreateListenerCollection(EmptyUser, this.WhenAnyValue(x => x.SelectedAuthor));
            Assignees = trackingAssignees.CreateListenerCollection(EmptyUser, this.WhenAnyValue(x => x.SelectedAssignee));

            CreatePullRequests();

            this.WhenAny(x => x.SelectedState, x => x.Value)
                .Where(x => PullRequests != null)
                .Subscribe(s => UpdateFilter(s, SelectedAssignee, SelectedAuthor, SearchQuery));

            this.WhenAny(x => x.SelectedAssignee, x => x.Value)
                .Where(x => PullRequests != null && x != EmptyUser)
                .Subscribe(a => UpdateFilter(SelectedState, a, SelectedAuthor, SearchQuery));

            this.WhenAny(x => x.SelectedAuthor, x => x.Value)
                .Where(x => PullRequests != null && x != EmptyUser)
                .Subscribe(a => UpdateFilter(SelectedState, SelectedAssignee, a, SearchQuery));

            this.WhenAny(x => x.SearchQuery, x => x.Value)
                .Where(x => PullRequests != null)
                .Subscribe(f => UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor, f));

            this.WhenAnyValue(x => x.SelectedRepository)
                .Skip(1)
                .Subscribe(_ => ResetAndLoad());

            OpenPullRequest = ReactiveCommand.Create();
            OpenPullRequest.Subscribe(DoOpenPullRequest);
            CreatePullRequest = ReactiveCommand.Create();
            CreatePullRequest.Subscribe(_ => DoCreatePullRequest());

            OpenPullRequestOnGitHub = ReactiveCommand.Create();
            OpenPullRequestOnGitHub.Subscribe(x => DoOpenPullRequestOnGitHub((int)x));

            // Get the current pull request session and the selected repository. When the session's
            // repository is the same as our selected repository set CheckedOutPullRequest to the
            // current session's model, so that the checked out PR can be highlighted.
            Observable.CombineLatest(
                sessionManager.WhenAnyValue(x => x.CurrentSession),
                this.WhenAnyValue(x => x.SelectedRepository),
                (s, r) => new { Session = s, Repository = r })
                .Subscribe(x =>
                {
                    CheckedOutPullRequest = x.Session?.RepositoryOwner == x.Repository?.Owner ?
                        x.Session?.PullRequest : null;
                });

            constructing = false;
        }

        public async Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection)
        {
            IsLoading = true;

            try
            {
                modelService = await modelServiceFactory.CreateAsync(connection);
                listSettings = settings.UIState
                    .GetOrCreateRepositoryState(repository.CloneUrl)
                    .PullRequests;
                localRepository = repository;
                remoteRepository = await modelService.GetRepository(
                    localRepository.Owner,
                    localRepository.Name);
                Repositories = remoteRepository.IsFork ?
                    new[] { remoteRepository.Parent, remoteRepository } :
                    new[] { remoteRepository };
                SelectedState = States.FirstOrDefault(x => x.Name == listSettings.SelectedState) ?? States[0];

                // Setting SelectedRepository will cause a Load().
                SelectedRepository = Repositories[0];
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override Task Refresh() => Load();

        Task Load()
        {
            IsBusy = true;

            PullRequests = modelService.GetPullRequests(SelectedRepository, pullRequests);
            pullRequests.Subscribe(pr =>
            {
                trackingAssignees.AddItem(pr.Assignee);
                trackingAuthors.AddItem(pr.Author);
            }, () => { });

            pullRequests.OriginalCompleted
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<System.Reactive.Unit, Exception>(ex =>
                {
                    // Occurs on network error, when the repository was deleted on GitHub etc.
                    log.Error(ex, "Received Exception reading pull requests");
                    return Observable.Empty<System.Reactive.Unit>();
                })
                .Subscribe(_ =>
                {
                    if (listSettings.SelectedAuthor != null)
                    {
                        SelectedAuthor = Authors.FirstOrDefault(x => x.Login == listSettings.SelectedAuthor);
                    }

                    if (listSettings.SelectedAssignee != null)
                    {
                        SelectedAssignee = Assignees.FirstOrDefault(x => x.Login == listSettings.SelectedAssignee);
                    }

                    IsBusy = false;
                    UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor, SearchQuery);
                });
            return Task.CompletedTask;
        }

        void UpdateFilter(PullRequestState state, IAccount ass, IAccount aut, string filText)
        {
            if (PullRequests == null)
                return;

            var filterTextIsNumber = false;
            var filterTextIsString = false;
            var filterPullRequestNumber = 0;

            if (filText != null)
            {
                filText = filText.Trim();

                var hasText = !string.IsNullOrEmpty(filText);

                if (hasText && filText.StartsWith("#", StringComparison.CurrentCultureIgnoreCase))
                {
                    filterTextIsNumber = int.TryParse(filText.Substring(1), out filterPullRequestNumber);
                }
                else
                {
                    filterTextIsNumber = int.TryParse(filText, out filterPullRequestNumber);
                }

                filterTextIsString = hasText && !filterTextIsNumber;
            }

            if (!pullRequests.Disposed)
            {
                pullRequests.Filter = (pullRequest, index, list) =>
                    (!state.IsOpen.HasValue || state.IsOpen == pullRequest.IsOpen) &&
                    (ass == null || ass.Equals(pullRequest.Assignee)) &&
                    (aut == null || aut.Equals(pullRequest.Author)) &&
                    (filterTextIsNumber == false || pullRequest.Number == filterPullRequestNumber) &&
                    (filterTextIsString == false || pullRequest.Title.ToUpperInvariant().Contains(filText.ToUpperInvariant()));
            }

            SaveSettings();
        }

        string searchQuery;
        public string SearchQuery
        {
            get { return searchQuery; }
            set { this.RaiseAndSetIfChanged(ref searchQuery, value); }
        }

        IReadOnlyList<IRemoteRepositoryModel> repositories;
        public IReadOnlyList<IRemoteRepositoryModel> Repositories
        {
            get { return repositories; }
            private set { this.RaiseAndSetIfChanged(ref repositories, value); }
        }

        IRemoteRepositoryModel selectedRepository;
        public IRemoteRepositoryModel SelectedRepository
        {
            get { return selectedRepository; }
            set { this.RaiseAndSetIfChanged(ref selectedRepository, value); }
        }

        ITrackingCollection<IPullRequestModel> pullRequests;
        public ITrackingCollection<IPullRequestModel> PullRequests
        {
            get { return pullRequests; }
            private set { this.RaiseAndSetIfChanged(ref pullRequests, value); }
        }

        IPullRequestModel selectedPullRequest;
        public IPullRequestModel SelectedPullRequest
        {
            get { return selectedPullRequest; }
            set { this.RaiseAndSetIfChanged(ref selectedPullRequest, value); }
        }

        PullRequestDetailModel checkedOutPullRequest;
        public PullRequestDetailModel CheckedOutPullRequest
        {
            get { return checkedOutPullRequest; }
            set { this.RaiseAndSetIfChanged(ref checkedOutPullRequest, value); }
        }

        IReadOnlyList<PullRequestState> states;
        public IReadOnlyList<PullRequestState> States
        {
            get { return states; }
            set { this.RaiseAndSetIfChanged(ref states, value); }
        }

        PullRequestState selectedState;
        public PullRequestState SelectedState
        {
            get { return selectedState; }
            set { this.RaiseAndSetIfChanged(ref selectedState, value); }
        }

        ObservableCollection<IAccount> assignees;
        public ObservableCollection<IAccount> Assignees
        {
            get { return assignees; }
            set { this.RaiseAndSetIfChanged(ref assignees, value); }
        }

        ObservableCollection<IAccount> authors;
        public ObservableCollection<IAccount> Authors
        {
            get { return authors; }
            set { this.RaiseAndSetIfChanged(ref authors, value); }
        }

        IAccount selectedAuthor;
        public IAccount SelectedAuthor
        {
            get { return selectedAuthor; }
            set { this.RaiseAndSetIfChanged(ref selectedAuthor, value); }
        }

        IAccount selectedAssignee;
        public IAccount SelectedAssignee
        {
            get { return selectedAssignee; }
            set { this.RaiseAndSetIfChanged(ref selectedAssignee, value); }
        }

        IAccount emptyUser = new Account("[None]", false, false, 0, 0, string.Empty, Observable.Empty<BitmapSource>());
        public IAccount EmptyUser
        {
            get { return emptyUser; }
        }

        Uri webUrl;
        public Uri WebUrl
        {
            get { return webUrl; }
            private set { this.RaiseAndSetIfChanged(ref webUrl, value); }
        }

        public bool IsSearchEnabled => true;

        public ReactiveCommand<object> OpenPullRequest { get; }
        public ReactiveCommand<object> CreatePullRequest { get; }
        public ReactiveCommand<object> OpenPullRequestOnGitHub { get; }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (disposed) return;
                pullRequests.Dispose();
                trackingAuthors.Dispose();
                trackingAssignees.Dispose();
                disposed = true;
            }
        }

        void CreatePullRequests()
        {
            PullRequests = new TrackingCollection<IPullRequestModel>();
            pullRequests.Comparer = OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.UpdatedAt).Compare;
            pullRequests.NewerComparer = OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.UpdatedAt).Compare;
        }

        void ResetAndLoad()
        {
            WebUrl = SelectedRepository.CloneUrl?.ToRepositoryUrl().Append("pulls");
            CreatePullRequests();
            UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor, SearchQuery);
            Load().Forget();
        }

        void SaveSettings()
        {
            if (!constructing)
            {
                listSettings.SelectedState = SelectedState.Name;
                listSettings.SelectedAssignee = SelectedAssignee?.Login;
                listSettings.SelectedAuthor = SelectedAuthor?.Login;
                settings.Save();
            }
        }

        void DoOpenPullRequest(object pullRequest)
        {
            Guard.ArgumentNotNull(pullRequest, nameof(pullRequest));

            var number = (int)pullRequest;
            NavigateTo(Invariant($"{SelectedRepository.Owner}/{SelectedRepository.Name}/pull/{number}"));
        }

        void DoCreatePullRequest()
        {
            NavigateTo("pull/new");
        }

        void DoOpenPullRequestOnGitHub(int pullRequest)
        {
            var repoUrl = SelectedRepository.CloneUrl.ToRepositoryUrl();
            var url = repoUrl.Append("pull/" + pullRequest);
            visualStudioBrowser.OpenUrl(url);
        }
    }
}
