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
        int number;
        IAccount author;
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

            OpenOnGitHub = ReactiveCommand.Create();
        }

        public int Number
        {
            get { return number; }
            private set { this.RaiseAndSetIfChanged(ref number, value); }
        }

        public IAccount Author
        {
            get { return author; }
            private set { this.RaiseAndSetIfChanged(ref author, value); }
        }

        public string Body
        {
            get { return body; }
            private set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        public ReactiveCommand<object> OpenOnGitHub { get; }

        public override void Initialize([AllowNull] ViewWithData data)
        {
            var prNumber = (int)data.Data;
            IsBusy = true;
            repositoryHost.ModelService.GetPullRequest(repository, prNumber)
                .Finally(() => IsBusy = false)
                .Subscribe(Load);
        }

        void Load(IPullRequestDetailModel model)
        {
            Number = model.Number;
            Title = model.Title;
            Author = model.Author;
            Body = model.Body;
        }
    }
}
