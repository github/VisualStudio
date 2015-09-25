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

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PullRequestList)]
    public class PullRequestListViewModel : BaseViewModel, IPullRequestListViewModel
    {
        [ImportingConstructor]
        PullRequestListViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap, ITeamExplorerServiceHolder teservice)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo)
        {}

        public PullRequestListViewModel(IRepositoryHost repositoryHost, ISimpleRepositoryModel repository)
        {
            pullRequests = repositoryHost.ModelService.GetPullRequests(repository)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.PullRequests, initialValue: new IPullRequestModel[] { });

            this.WhenAny(x => x.PullRequests, x => x.Value)
                .Where(pr => pr.Any())
                .Subscribe(pr => SelectedPullRequest = pr.FirstOrDefault());
        }

        readonly ObservableAsPropertyHelper<IReadOnlyList<IPullRequestModel>> pullRequests;
        public IReadOnlyList<IPullRequestModel> PullRequests => pullRequests.Value;

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
