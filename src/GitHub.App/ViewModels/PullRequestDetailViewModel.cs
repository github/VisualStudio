using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using NullGuard;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType = UIViewType.PRDetail)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class PullRequestDetailViewModel : BaseViewModel, IPullRequestDetailViewModel
    {
        readonly IRepositoryHost repositoryHost;
        readonly ILocalRepositoryModel repository;
        PullRequestState state;
        string sourceBranchDisplayName;
        string targetBranchDisplayName;
        int commitCount;
        int filesChangedCount;
        IAccount author;
        DateTimeOffset createdAt;
        string body;
        int number;

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

        public PullRequestState State
        {
            get { return state; }
            private set { this.RaiseAndSetIfChanged(ref state, value); }
        }

        public string SourceBranchDisplayName
        {
            get { return sourceBranchDisplayName; }
            private set { this.RaiseAndSetIfChanged(ref sourceBranchDisplayName, value); }
        }

        public string TargetBranchDisplayName
        {
            get { return targetBranchDisplayName; }
            private set { this.RaiseAndSetIfChanged(ref targetBranchDisplayName, value); }
        }

        public int CommitCount
        {
            get { return commitCount; }
            private set { this.RaiseAndSetIfChanged(ref commitCount, value); }
        }

        public int FilesChangedCount
        {
            get { return filesChangedCount; }
            private set { this.RaiseAndSetIfChanged(ref filesChangedCount, value); }
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

        public DateTimeOffset CreatedAt
        {
            get { return createdAt; }
            private set { this.RaiseAndSetIfChanged(ref createdAt, value); }
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
            repositoryHost.ApiClient.GetPullRequest(repository.Owner, repository.CloneUrl.RepositoryName, prNumber)
                .Finally(() => IsBusy = false)
                .Subscribe(Load);
        }

        void Load(PullRequest pullRequest)
        {
            State = CreatePullRequestState(pullRequest);
            SourceBranchDisplayName = GetBranchDisplayName(pullRequest.Head.Label);
            TargetBranchDisplayName = GetBranchDisplayName(pullRequest.Base.Label);
            CommitCount = pullRequest.Commits;
            FilesChangedCount = pullRequest.ChangedFiles;
            Title = pullRequest.Title;
            Number = pullRequest.Number;
            Author = new Models.Account(pullRequest.User);
            CreatedAt = pullRequest.CreatedAt;
            Body = pullRequest.Body;
        }

        static PullRequestState CreatePullRequestState(PullRequest pullRequest)
        {
            if (pullRequest.State == ItemState.Open)
            {
                return new PullRequestState(true, "Open");
            }
            else if (pullRequest.Merged)
            {
                return new PullRequestState(false, "Merged");
            }
            else
            {
                return new PullRequestState(false, "Closed");
            }
        }

        string GetBranchDisplayName(string targetBranchLabel)
        {
            var parts = targetBranchLabel.Split(':');
            var owner = parts[0];
            return owner == repository.CloneUrl.Owner ? parts[1] : targetBranchLabel;
        }
    }
}
