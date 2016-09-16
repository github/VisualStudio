using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRDetail)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class PullRequestDetailViewModel : BaseViewModel, IPullRequestDetailViewModel
    {
        readonly IRepositoryHost repositoryHost;
        readonly ILocalRepositoryModel repository;
        string body;

        [ImportingConstructor]
        PullRequestDetailViewModel(
            IConnectionRepositoryHostMap connectionRepositoryHostMap,
            ITeamExplorerServiceHolder teservice)
            : this(connectionRepositoryHostMap.CurrentRepositoryHost, teservice.ActiveRepo)
        {
        }

        public PullRequestDetailViewModel(
            IRepositoryHost repositoryHost,
            ILocalRepositoryModel repository)
        {
            this.repositoryHost = repositoryHost;
            this.repository = repository;
        }

        public string Body
        {
            get { return body; }
            private set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        public override void Initialize([AllowNull] ViewWithData data)
        {
            var number = (int)data.Data;
            IsBusy = true;
            repositoryHost.ModelService.GetPullRequest(repository, number)
                .Finally(() => IsBusy = false)
                .Subscribe(Load);
        }

        void Load(IPullRequestDetailModel model)
        {
            Title = model.Title;
            Body = model.Body;
        }
    }
}
