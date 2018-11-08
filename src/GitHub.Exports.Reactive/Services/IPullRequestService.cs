using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using LibGit2Sharp;

namespace GitHub.Services
{
    public interface IPullRequestService
    {
        /// <summary>
        /// Reads a page of pull request items.
        /// </summary>
        /// <param name="address">The host address.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="after">The end cursor of the previous page, or null for the first page.</param>
        /// <param name="states">The pull request states to filter by</param>
        /// <returns>A page of pull request item models.</returns>
        Task<Page<PullRequestListItemModel>> ReadPullRequests(
            HostAddress address,
            string owner,
            string name,
            string after,
            PullRequestStateEnum[] states);

        /// <summary>
        /// Reads a page of users that can be assigned to pull requests.
        /// </summary>
        /// <param name="address">The host address.</param>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="after">The end cursor of the previous page, or null for the first page.</param>
        /// <returns>A page of author models.</returns>
        Task<Page<ActorModel>> ReadAssignableUsers(
            HostAddress address,
            string owner,
            string name,
            string after);

        IObservable<IPullRequestModel> CreatePullRequest(IModelService modelService,
            LocalRepositoryModel sourceRepository, RepositoryModel targetRepository,
            BranchModel sourceBranch, BranchModel targetBranch,
            string title, string body);

        /// <summary>
        /// Checks whether the working directory for the specified repository is in a clean state.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        IObservable<bool> IsWorkingDirectoryClean(LocalRepositoryModel repository);

        /// <summary>
        /// Count the number of submodules that require syncing.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns>The number of submodules that need to be synced.</returns>
        IObservable<int> CountSubmodulesToSync(LocalRepositoryModel repository);

        /// <summary>
        /// Checks out a pull request to a local branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <param name="localBranchName">The name of the local branch.</param>
        /// <returns></returns>
        IObservable<Unit> Checkout(LocalRepositoryModel repository, PullRequestDetailModel pullRequest, string localBranchName);

        /// <summary>
        /// Carries out a pull on the current branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        IObservable<Unit> Pull(LocalRepositoryModel repository);

        /// <summary>
        /// Carries out a push of the current branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        IObservable<Unit> Push(LocalRepositoryModel repository);

        /// <summary>
        /// Sync submodules on the current branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="progress">A method that will be called with progress messages</param>
        Task<bool> SyncSubmodules(LocalRepositoryModel repository, Action<string> progress);

        /// <summary>
        /// Calculates the name of a local branch for a pull request avoiding clashes with existing branches.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        /// <param name="pullRequestTitle">The pull request title.</param>
        /// <returns></returns>
        IObservable<string> GetDefaultLocalBranchName(LocalRepositoryModel repository, int pullRequestNumber, string pullRequestTitle);

        /// <summary>
        /// Gets the local branches that exist for the specified pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <returns></returns>
        IObservable<BranchModel> GetLocalBranches(LocalRepositoryModel repository, PullRequestDetailModel pullRequest);

        /// <summary>
        /// Ensures that all local branches for the specified pull request are marked as PR branches.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <returns>
        /// An observable that produces a single value indicating whether a change to the repository was made.
        /// </returns>
        /// <remarks>
        /// Pull request branches are marked in the local repository with a config value so that they can
        /// be easily identified without a roundtrip to the server. This method ensures that the local branches
        /// for the specified pull request are indeed marked and returns a value indicating whether any branches
        /// have had the mark added.
        /// </remarks>
        IObservable<bool> EnsureLocalBranchesAreMarkedAsPullRequests(LocalRepositoryModel repository, PullRequestDetailModel pullRequest);

        /// <summary>
        /// Determines whether the specified pull request is from the specified repository.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <returns></returns>
        bool IsPullRequestFromRepository(LocalRepositoryModel repository, PullRequestDetailModel pullRequest);

        /// <summary>
        /// Switches to an existing branch for the specified pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <returns></returns>
        IObservable<Unit> SwitchToBranch(LocalRepositoryModel repository, PullRequestDetailModel pullRequest);

        /// <summary>
        /// Gets the history divergence between the current HEAD and the specified pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        /// <returns></returns>
        IObservable<BranchTrackingDetails> CalculateHistoryDivergence(LocalRepositoryModel repository, int pullRequestNumber);

        /// <summary>
        /// Gets the SHA of the merge base for a pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <returns></returns>
        Task<string> GetMergeBase(LocalRepositoryModel repository, PullRequestDetailModel pullRequest);

        /// <summary>
        /// Gets the changes between the pull request base and head.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <returns></returns>
        IObservable<TreeChanges> GetTreeChanges(LocalRepositoryModel repository, PullRequestDetailModel pullRequest);

        /// <summary>
        /// Gets the pull request associated with the current branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns>
        /// An observable that produces a single tuple which contains the owner of the fork and the
        /// pull request number. Returns null if the current branch is not a PR branch.
        /// </returns>
        /// <remarks>
        /// This method does not do an API request - it simply checks the mark left in the git
        /// config by <see cref="Checkout(LocalRepositoryModel, PullRequestDetailModel, string)"/>.
        /// </remarks>
        IObservable<(string owner, int number)> GetPullRequestForCurrentBranch(LocalRepositoryModel repository);

        /// <summary>
        /// Gets the encoding for the specified local file.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="relativePath">The relative path to the file in the repository.</param>
        /// <returns>
        /// The file's encoding or null if the file doesn't exist locally.
        /// </returns>
        Encoding GetEncoding(LocalRepositoryModel repository, string relativePath);

        /// <summary>
        /// Extracts a file at the specified commit to a temporary file.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <param name="relativePath">The path to the file, relative to the repository root.</param>
        /// <param name="commitSha">The SHA of the commit.</param>
        /// <param name="encoding">
        /// The encoding to save the file with. If null, will use the file's original encoding.
        /// </param>
        /// <returns>The path to the temporary file.</returns>
        Task<string> ExtractToTempFile(
            LocalRepositoryModel repository,
            PullRequestDetailModel pullRequest,
            string relativePath,
            string commitSha,
            Encoding encoding);

        /// <summary>
        /// Remotes all unused remotes that were created by GitHub for Visual Studio to track PRs
        /// from forks.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        IObservable<Unit> RemoveUnusedRemotes(LocalRepositoryModel repository);

        IObservable<string> GetPullRequestTemplate(LocalRepositoryModel repository);

        /// <summary>
        /// Gets the unique commits from <paramref name="compareBranch"/> to the merge base of 
        /// <paramref name="baseBranch"/> and <paramref name="compareBranch"/> and returns their
        /// commit messages.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="baseBranch">The base branch to find a merge base with.</param>
        /// <param name="compareBranch">The compare branch to find a merge base with.</param>
        /// <param name="maxCommits">The maximum number of commits to return.</param>
        /// <returns>An enumerable of unique commits from the merge base to the compareBranch.</returns>
        IObservable<IReadOnlyList<CommitMessage>> GetMessagesForUniqueCommits(
            LocalRepositoryModel repository,
            string baseBranch,
            string compareBranch,
            int maxCommits);

        /// <summary>
        /// Displays a confirmation diaglog to ask if the user wants to cancel a pending review.
        /// </summary>
        /// <returns></returns>
        bool ConfirmCancelPendingReview();
    }
}
