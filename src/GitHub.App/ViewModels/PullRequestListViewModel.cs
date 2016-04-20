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
using System.ComponentModel;
using GitHub.Collections;
using System.Windows.Input;
using GitHub.UI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRList)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestListViewModel : BaseViewModel, IPullRequestListViewModel
    {
        readonly ReactiveCommand<object> openPullRequestCommand;
        readonly IRepositoryHost repositoryHost;
        readonly ISimpleRepositoryModel repository;

        [ImportingConstructor]
        PullRequestListViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap, ITeamExplorerServiceHolder teservice)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo)
        { }

        public PullRequestListViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel repository)
        {
            this.repositoryHost = repositoryHost;
            this.repository = repository;

            CancelCommand = ReactiveCommand.Create();
            openPullRequestCommand = ReactiveCommand.Create();
            openPullRequestCommand.Subscribe(_ =>
            {
                VisualStudio.Services.DefaultExportProvider.GetExportedValue<IVisualStudioBrowser>().OpenUrl(repositoryHost.Address.WebUri);
            });

            var list = new TrackingCollection<IPullRequestModel>();
            list.Comparer = OrderedComparer<IPullRequestModel>.OrderByDescending(x => x.UpdatedAt).Compare;
            list.Filter = (pr, i, l) => pr.IsOpen;
            PullRequests = list;
        }

        public override void Initialize([AllowNull] ViewWithData data)
        {
            base.Initialize(data);

            var old = PullRequests;
            var list = repositoryHost.ModelService.GetPullRequests(repository);
            list.Comparer = old.Comparer;
            list.Filter = old.Filter;
            PullRequests = list;
            list.Subscribe();
            old.Dispose();
        }

        ITrackingCollection<IPullRequestModel> pullRequests;
        public ITrackingCollection<IPullRequestModel> PullRequests
        {
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
    }
}
