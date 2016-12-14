using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GitHub.App;
using GitHub.Collections;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using GitHub.Settings;
using GitHub.UI;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRList)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestListViewModel : BaseViewModel, IPullRequestListViewModel, IDisposable
    {
        readonly ReactiveCommand<object> openPullRequestCommand;
        readonly IRepositoryHost repositoryHost;
        readonly ILocalRepositoryModel repository;
        readonly TrackingCollection<IAccount> trackingAuthors;
        readonly TrackingCollection<IAccount> trackingAssignees;
        readonly IPackageSettings settings;
        readonly PullRequestListUIState listSettings;

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
            this.repositoryHost = repositoryHost;
            this.repository = repository;
            this.settings = settings;

            Title = Resources.PullRequestsNavigationItemText;

            this.listSettings = settings.UIState
                .GetOrCreateRepositoryState(repository.CloneUrl)
                .PullRequests;

            openPullRequestCommand = ReactiveCommand.Create();
            openPullRequestCommand.Subscribe(_ =>
            {
                VisualStudio.Services.DefaultExportProvider.GetExportedValue<IVisualStudioBrowser>().OpenUrl(repositoryHost.Address.WebUri);
            });

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

            PullRequests = new TrackingCollection<IPullRequestModel>();
            pullRequests.Comparer = OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.UpdatedAt).Compare;
            pullRequests.NewerComparer = OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.UpdatedAt).Compare;

            this.WhenAny(x => x.SelectedState, x => x.Value)
                .Where(x => PullRequests != null)
                .Subscribe(s => UpdateFilter(s, SelectedAssignee, SelectedAuthor, FilterText));

            this.WhenAny(x => x.SelectedAssignee, x => x.Value)
                .Where(x => PullRequests != null && x != EmptyUser && IsLoaded)
                .Subscribe(a => UpdateFilter(SelectedState, a, SelectedAuthor, FilterText));

            this.WhenAny(x => x.SelectedAuthor, x => x.Value)
                .Where(x => PullRequests != null && x != EmptyUser && IsLoaded)
                .Subscribe(a => UpdateFilter(SelectedState, SelectedAssignee, a, FilterText));

            this.WhenAny(x => x.FilterText, x => x.Value)
                .Where(x => PullRequests != null && IsLoaded)
                .Subscribe(f => UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor, f));

            SelectedState = States.FirstOrDefault(x => x.Name == listSettings.SelectedState) ?? States[0];
        }

        public override void Initialize([AllowNull] ViewWithData data)
        {
            base.Initialize(data);

            IsLoaded = false;

            PullRequests = repositoryHost.ModelService.GetPullRequests(repository, pullRequests);
            pullRequests.Subscribe(pr =>
            {
                trackingAssignees.AddItem(pr.Assignee);
                trackingAuthors.AddItem(pr.Author);
            }, () => { });

            pullRequests.OriginalCompleted
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<System.Reactive.Unit, Octokit.AuthorizationException>(ex =>
                {
                    // TODO: Do some decent logging here
                    return repositoryHost.LogOut();
                })
                .Catch<System.Reactive.Unit, Octokit.NotFoundException>(ex =>
                {
                    //this is caused when repository was deleted on github
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
 
                    IsLoaded = true;
                    UpdateFilter(SelectedState, SelectedAssignee, SelectedAuthor, FilterText);
                });
        }

        void UpdateFilter(PullRequestState state, [AllowNull]IAccount ass = null, [AllowNull]IAccount aut = null, [AllowNull]string filText = null)
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

                if (hasText && filterText.StartsWith("#", StringComparison.CurrentCultureIgnoreCase))
                {
                    filterTextIsNumber = int.TryParse(filText.Substring(1), out filterPullRequestNumber);
                }
                else
                {
                    filterTextIsNumber = int.TryParse(filText, out filterPullRequestNumber);
                }

                filterTextIsString = hasText && !filterTextIsNumber;
            }

            pullRequests.Filter = (pullRequest, index, list) =>
                (!state.IsOpen.HasValue || state.IsOpen == pullRequest.IsOpen) &&
                (ass == null || ass.Equals(pullRequest.Assignee)) &&
                (aut == null || aut.Equals(pullRequest.Author)) &&
                (filterTextIsNumber == false || pullRequest.Number == filterPullRequestNumber) && 
                (filterTextIsString == false || pullRequest.Title.ToUpperInvariant().Contains(filText.ToUpperInvariant()));
        }

        string filterText;
        [AllowNull]
        public string FilterText
        {
            get { return filterText; }
            set { this.RaiseAndSetIfChanged(ref filterText, value); }
        }

        bool isLoaded;
        public bool IsLoaded
        {
            get { return isLoaded; }
            private set { this.RaiseAndSetIfChanged(ref isLoaded, value); }
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

        public ICommand OpenPullRequest
        {
            get { return openPullRequestCommand; }
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

        IAccount emptyUser = new Account("[None]", false, false, 0, 0, Observable.Empty<BitmapSource>());
        public IAccount EmptyUser
        {
            get { return emptyUser; }
        }

        bool disposed;
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposed) return;
                pullRequests.Dispose();
                trackingAuthors.Dispose();
                trackingAssignees.Dispose();
                SaveSettings();
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void SaveSettings()
        {
            listSettings.SelectedState = SelectedState.Name;
            listSettings.SelectedAssignee = SelectedAssignee?.Login;
            listSettings.SelectedAuthor = SelectedAuthor?.Login;
            settings.Save();
        }
    }
}
