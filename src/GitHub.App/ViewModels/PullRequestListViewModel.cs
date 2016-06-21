using GitHub.Exports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;
using System.Collections.ObjectModel;
using ReactiveUI;
using NullGuard;
using System.ComponentModel.Composition;
using GitHub.Services;
using System.Reactive.Linq;
using GitHub.Extensions.Reactive;
using System.Windows.Data;
using GitHub.Collections;
using System.Windows.Input;
using GitHub.UI;
using System.Windows.Media.Imaging;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRList)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestListViewModel : BaseViewModel, IPullRequestListViewModel, IDisposable
    {
        readonly ReactiveCommand<object> openPullRequestCommand;
        readonly IRepositoryHost repositoryHost;
        readonly ISimpleRepositoryModel repository;
        readonly TrackingCollection<IAccount> trackingAuthors;
        readonly TrackingCollection<IAccount> trackingAssignees;

        [ImportingConstructor]
        PullRequestListViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap, ITeamExplorerServiceHolder teservice)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo)
        { }

        public PullRequestListViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel repository)
        {
            this.repositoryHost = repositoryHost;
            this.repository = repository;

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
            SelectedState = States[0];

            this.WhenAny(x => x.SelectedState, x => x.Value)
                .Where(x => PullRequests != null)
                .Subscribe(s => UpdateFilter(s, SelectedAssignee, SelectedAuthor));

            this.WhenAny(x => x.SelectedAssignee, x => x.Value)
                .Where(x => PullRequests != null)
                .Subscribe(a => UpdateFilter(SelectedState, a, SelectedAuthor));

            this.WhenAny(x => x.SelectedAuthor, x => x.Value)
                .Where(x => PullRequests != null)
                .Subscribe(a => UpdateFilter(SelectedState, SelectedAssignee, a));

            trackingAuthors = new TrackingCollection<IAccount>(Observable.Empty<IAccount>(),
                OrderedComparer<IAccount>.OrderByDescending(x => x.Login).Compare);
            trackingAssignees = new TrackingCollection<IAccount>(Observable.Empty<IAccount>(), 
                OrderedComparer<IAccount>.OrderByDescending(x => x.Login).Compare);
            trackingAuthors.Subscribe();
            trackingAssignees.Subscribe();

            Authors = trackingAuthors.CreateListenerCollection(new List<IAccount> { EmptyUser });
            Assignees = trackingAssignees.CreateListenerCollection(new List<IAccount> { EmptyUser });

            PullRequests = new TrackingCollection<IPullRequestModel>();
            pullRequests.Comparer = OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.UpdatedAt).Compare;
            pullRequests.Filter = (pr, i, l) => pr.IsOpen;
        }

        public override void Initialize([AllowNull] ViewWithData data)
        {
            base.Initialize(data);

            repositoryHost.ModelService.GetPullRequests(repository, pullRequests);
            pullRequests.Subscribe(pr =>
            {
                trackingAssignees.AddItem(pr.Assignee);
                trackingAuthors.AddItem(pr.Author);
            }, () => { });
        }

        void UpdateFilter(PullRequestState state, [AllowNull]IAccount ass, [AllowNull]IAccount aut)
        {
            if (PullRequests == null)
                return;
            pullRequests.Filter = (pr, i, l) =>
                (!state.IsOpen.HasValue || state.IsOpen == pr.IsOpen) &&
                     (ass == null || ass.Equals(pr.Assignee)) &&
                     (aut == null || aut.Equals(pr.Author));
        }

        TrackingCollection<IPullRequestModel> pullRequests;
        public ObservableCollection<IPullRequestModel> PullRequests
        {
            [return: AllowNull]
            get { return pullRequests; }
            private set { this.RaiseAndSetIfChanged(ref pullRequests, (TrackingCollection<IPullRequestModel>)value); }
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
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    // doing this as an extension because I get the feeling it might be useful in other places
    public static class TrackingCollectionExtensions
    {
        public static ObservableCollection<T> CreateListenerCollection<T>(this ITrackingCollection<T> tcol,
            IList<T> stickieItemsOnTop = null)
            where T : ICopyable<T>
        {
            var col = new ObservableCollection<T>();
            tcol.CollectionChanged += (s, e) =>
            {
                var offset = 0;
                if (stickieItemsOnTop != null)
                {
                    foreach (var item in stickieItemsOnTop)
                    {
                        if (col.Contains(item))
                            offset++;
                    }
                }

                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
                {
                    for (int i = 0, oldIdx = e.OldStartingIndex, newIdx = e.NewStartingIndex;
                        i < e.OldItems.Count; i++, oldIdx++, newIdx++)
                        col.Move(oldIdx + offset, newIdx + offset);
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    foreach (T item in e.NewItems)
                        col.Add(item);
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    foreach (T item in e.OldItems)
                        col.Remove(item);
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                    for (int i = 0, idx = e.OldStartingIndex;
                        i < e.OldItems.Count; i++, idx++)
                        col[idx + offset] = (T)e.NewItems[i];
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    col.Clear();
                    if (stickieItemsOnTop != null)
                    {
                        foreach (var item in stickieItemsOnTop)
                            col.Add(item);
                    }
                }
            };
            return col;
        }
    }
}
