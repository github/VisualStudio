using GitHub.Models;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base class for pull request view models.
    /// </summary>
    public interface IPullRequestViewModelBase : IIssueishViewModel
    {
        /// <summary>
        /// Gets the local repository.
        /// </summary>
        ILocalRepositoryModel LocalRepository { get; }

        /// <summary>
        /// Gets the pull request state.
        /// </summary>
        PullRequestState State { get; }

        /// <summary>
        /// Gets a the pull request's source (head) branch display.
        /// </summary>
        string SourceBranchDisplayName { get; }

        /// <summary>
        /// Gets a the pull request's target (base) branch display.
        /// </summary>
        string TargetBranchDisplayName { get; }
    }
}