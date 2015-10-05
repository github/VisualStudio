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

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PullRequestList)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestListViewModel : BaseViewModel, IPullRequestListViewModel
    {
        [ImportingConstructor]
        PullRequestListViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap, ITeamExplorerServiceHolder teservice)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo)
        {}

        public PullRequestListViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel repository)
        {
            var list = repositoryHost.ModelService.GetPullRequests(repository);
            PullRequests = list.CreateDerivedCollection(x => x, pr => list.IndexOf(pr) < 10, OrderedComparer<IPullRequestModel>.OrderBy(x => x.UpdatedAt).Compare);
            list.Subscribe();

            this.WhenAny(x => x.PullRequests, x => x.Value)
                .Where(pr => pr.Any())
                .Subscribe(pr => SelectedPullRequest = pr.FirstOrDefault());
        }

        IReactiveDerivedList<IPullRequestModel> pullRequests;
        public IReactiveDerivedList<IPullRequestModel> PullRequests
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
    }
}
