using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GitHub.App;
using GitHub.Collections;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using GitHub.UI;
using NLog;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRList)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestListViewModel : PanePageViewModelBase, IPullRequestListViewModel, IDisposable
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        readonly IRepositoryHost repositoryHost;
        readonly ILocalRepositoryModel localRepository;
        readonly TrackingCollection<IAccount> trackingAuthors;
        readonly TrackingCollection<IAccount> trackingAssignees;
        readonly IPackageSettings settings;
        readonly PullRequestListUIState listSettings;
        readonly bool constructing;
        IRemoteRepositoryModel remoteRepository;

        [ImportingConstructor]
        PullRequestListViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            ITeamExplorerServiceHolder teservice,
            IPackageSettings settings)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo, settings)
        {
        }

        public PullRequestListViewModel(
            IRepositoryHost repositoryHost,
            ILocalRepositoryModel repository,
            IPackageSettings settings)
        {
            constructing = true;
            this.repositoryHost = repositoryHost;
            this.localRepository = repository;
            this.settings = settings;

            Title = Resources.PullRequestsNavigationItemText;

            this.listSettings = settings.UIState
                .GetOrCreateRepositoryState(repository.CloneUrl)
                .PullRequests;

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
                .Subscribe(s => UpdateFilter(s, SelectedAssignee, SelectedAuthor));

            this.WhenAny(x => x.SelectedAssignee, x => x.Value)
                .Where(x => PullRequests != null && x != EmptyUser)
                .Subscribe(a => UpdateFilter(SelectedState, a, SelectedAuthor));

            this.WhenAny(x => x.SelectedAuthor, x => x.Value)
                .Where(x => PullRequests != null && x != EmptyUser)
                .Subscribe(a => UpdateFilter(SelectedState, SelectedAssignee, a));

            this.WhenAnyValue(x => x.ShowPullRequestsForFork)
                .Skip(1)
                .Subscribe(_ => ResetAndLoad());

            SelectedState = States.FirstOrDefault(x => x.Name == listSettings.SelectedState) ?? States[0];
            OpenPullRequest = ReactiveCommand.Create();
            OpenPullRequest.Subscribe(DoOpenPullRequest);
            CreatePullRequest = ReactiveCommand.Create();
            CreatePullRequest.Subscribe(_ => DoCreatePullRequest());

            constructing = false;
        }

        public override void Initialize([AllowNull] ViewWithData data)
        {
            base.Initialize(data);
            Load().Forget();
        }

        IRemoteRepositoryModel Repository
        {
            get
            {
                if (remoteRepository != null)
                {
                    return !remoteRepository.IsFork || showForkPullRequests ?
                        remoteRepository : remoteRepository.Parent;
                }

                return null;
            }
        }

        async Task Load()
        {
            IsBusy = true;

            if (remoteRepository == null)
            {
                remoteRepository = await repositoryHost.ModelService.GetRepository(
                    localRepository.Owner,
                    localRepository.Name);
                RepositoryIsFork = remoteRepository.IsFork;
            }

            PullRequests = repositoryHost.ModelService.GetPullRequests(Repository, pullRequests);
            pullRequests.Subscribe(pr =>
            {
                trackingAssignees.AddItem(pr.Assignee);
                trackingAuthors.AddItem(pr.Author);
            }, () => { });

            pullRequests.OriginalCompleted
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<System.Reactive.Unit, Octokit.AuthorizationException>(ex =>
                {
                    log.Info("Received AuthorizationException reading pull requests", ex);
                    return repositoryHost.LogOut();
                })
                .Catch<System.Reactive.Unit, Exception>(ex =>
                {
                    // Occurs on network error, when the repository was deleted on GitHub etc.
                    log.Info("Received Exception reading pull requests", ex);
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
                    UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor);
                });
        }

        void UpdateFilter(PullRequestState state, [AllowNull]IAccount ass, [AllowNull]IAccount aut)
        {
            if (PullRequests == null)
                return;
            pullRequests.Filter = (pr, i, l) =>
                (!state.IsOpen.HasValue || state.IsOpen == pr.IsOpen) &&
                     (ass == null || ass.Equals(pr.Assignee)) &&
                     (aut == null || aut.Equals(pr.Author));
            SaveSettings();
        }

        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            private set { this.RaiseAndSetIfChanged(ref isBusy, value); }
        }

        ITrackingCollection<IPullRequestModel> pullRequests;
        public ITrackingCollection<IPullRequestModel> PullRequests
        {
            [return: AllowNull]
            get { return pullRequests; }
            private set { this.RaiseAndSetIfChanged(ref pullRequests, value); }
        }

        IPullRequestModel selectedPullRequest;
        [AllowNull]
        public IPullRequestModel SelectedPullRequest
        {
            [return: AllowNull]
            get { return selectedPullRequest; }
            set { this.RaiseAndSetIfChanged(ref selectedPullRequest, value); }
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
            [return: AllowNull]
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
        [AllowNull]
        public IAccount SelectedAuthor
        {
            [return: AllowNull]
            get { return selectedAuthor; }
            set { this.RaiseAndSetIfChanged(ref selectedAuthor, value); }
        }

        IAccount selectedAssignee;
        [AllowNull]
        public IAccount SelectedAssignee
        {
            [return: AllowNull]
            get { return selectedAssignee; }
            set { this.RaiseAndSetIfChanged(ref selectedAssignee, value); }
        }

        bool repositoryIsFork;
        public bool RepositoryIsFork
        {
            get { return repositoryIsFork; }
            set { this.RaiseAndSetIfChanged(ref repositoryIsFork, value); }
        }

        bool showForkPullRequests;
        public bool ShowPullRequestsForFork
        {
            get { return showForkPullRequests; }
            set { this.RaiseAndSetIfChanged(ref showForkPullRequests, value); }
        }

        IAccount emptyUser = new Account("[None]", false, false, 0, 0, Observable.Empty<BitmapSource>());
        public IAccount EmptyUser
        {
            get { return emptyUser; }
        }

        readonly Subject<ViewWithData> navigate = new Subject<ViewWithData>();
        public IObservable<ViewWithData> Navigate => navigate;

        public ReactiveCommand<object> OpenPullRequest { get; }
        public ReactiveCommand<object> CreatePullRequest { get; }

        bool disposed;
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;
                pullRequests.Dispose();
                trackingAuthors.Dispose();
                trackingAssignees.Dispose();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void CreatePullRequests()
        {
            PullRequests = new TrackingCollection<IPullRequestModel>();
            pullRequests.Comparer = OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.UpdatedAt).Compare;
            pullRequests.NewerComparer = OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.UpdatedAt).Compare;
        }

        void ResetAndLoad()
        {
            CreatePullRequests();
            UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor);
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
            var d = new ViewWithData(UIControllerFlow.PullRequestDetail)
            {
                Data = new PullRequestDetailArgument
                {
                    Repository = Repository,
                    Number = (int)pullRequest,
                }
            };
            navigate.OnNext(d);
        }

        void DoCreatePullRequest()
        {
            var d = new ViewWithData(UIControllerFlow.PullRequestCreation);
            navigate.OnNext(d);
        }
    }
}
