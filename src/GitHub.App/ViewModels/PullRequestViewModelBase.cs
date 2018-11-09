using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Logging;
using GitHub.Models;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base class for pull request view models.
    /// </summary>
    public class PullRequestViewModelBase : IssueishViewModel, IPullRequestViewModelBase
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestViewModelBase>();
        PullRequestState state;
        string sourceBranchDisplayName;
        string targetBranchDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestViewModelBase"/> class.
        /// </summary>
        [ImportingConstructor]
        public PullRequestViewModelBase()
        {
        }

        /// <inheritdoc/>
        public LocalRepositoryModel LocalRepository { get; private set; }

        public PullRequestState State
        {
            get => state;
            protected set => this.RaiseAndSetIfChanged(ref state, value);
        }

        public string SourceBranchDisplayName
        {
            get => sourceBranchDisplayName;
            private set => this.RaiseAndSetIfChanged(ref sourceBranchDisplayName, value);
        }

        public string TargetBranchDisplayName
        {
            get => targetBranchDisplayName;
            private set => this.RaiseAndSetIfChanged(ref targetBranchDisplayName, value);
        }

        protected virtual async Task InitializeAsync(
            RemoteRepositoryModel repository,
            LocalRepositoryModel localRepository,
            PullRequestDetailModel model)
        {
            await base.InitializeAsync(repository, model).ConfigureAwait(true);

            var fork = model.BaseRepositoryOwner != model.HeadRepositoryOwner;
            LocalRepository = localRepository;
            State = model.State;
            SourceBranchDisplayName = GetBranchDisplayName(fork, model.HeadRepositoryOwner, model.HeadRefName);
            TargetBranchDisplayName = GetBranchDisplayName(fork, model.BaseRepositoryOwner, model.BaseRefName);
        }

        static string GetBranchDisplayName(bool isFromFork, string owner, string label)
        {
            if (owner != null)
            {
                return isFromFork ? owner + ':' + label : label;
            }
            else
            {
                return Resources.InvalidBranchName;
            }
        }
    }
}
