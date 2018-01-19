using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;
using LibGit2Sharp;
using Octokit;
using IConnection = GitHub.Models.IConnection;

namespace GitHub.Services
{
    public interface IPullRequestService
    {
        IObservable<IPullRequestModel> CreatePullRequest(IModelService modelService,
            ILocalRepositoryModel sourceRepository, IRepositoryModel targetRepository,
            IBranch sourceBranch, IBranch targetBranch,
            string title, string body);

        /// <summary>
        /// Checks whether the working directory for the specified repository is in a clean state.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        IObservable<bool> IsWorkingDirectoryClean(ILocalRepositoryModel repository);

        /// <summary>
        /// Count the number of submodules that require syncing.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns>The number of submodules that need to be synced.</returns>
        IObservable<int> CountSubmodulesToSync(ILocalRepositoryModel repository);

        /// <summary>
        /// Checks out a pull request to a local branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <param name="localBranchName">The name of the local branch.</param>
        /// <returns></returns>
        IObservable<Unit> Checkout(ILocalRepositoryModel repository, IPullRequestModel pullRequest, string localBranchName);

        /// <summary>
        /// Carries out a pull on the current branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        IObservable<Unit> Pull(ILocalRepositoryModel repository);

        /// <summary>
        /// Carries out a push of the current branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        IObservable<Unit> Push(ILocalRepositoryModel repository);

        /// <summary>
        /// Sync submodules on the current branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        IObservable<Unit> SyncSubmodules(ILocalRepositoryModel repository);

        /// <summary>
        /// Calculates the name of a local branch for a pull request avoiding clashes with existing branches.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        /// <param name="pullRequestTitle">The pull request title.</param>
        /// <returns></returns>
        IObservable<string> GetDefaultLocalBranchName(ILocalRepositoryModel repository, int pullRequestNumber, string pullRequestTitle);

        /// <summary>
        /// Gets the local branches that exist for the specified pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <returns></returns>
        IObservable<IBranch> GetLocalBranches(ILocalRepositoryModel repository, IPullRequestModel pullRequest);

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
        IObservable<bool> EnsureLocalBranchesAreMarkedAsPullRequests(ILocalRepositoryModel repository, IPullRequestModel pullRequest);

        /// <summary>
        /// Determines whether the specified pull request is from the specified repository.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <returns></returns>
        bool IsPullRequestFromRepository(ILocalRepositoryModel repository, IPullRequestModel pullRequest);

        /// <summary>
        /// Switches to an existing branch for the specified pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <returns></returns>
        IObservable<Unit> SwitchToBranch(ILocalRepositoryModel repository, IPullRequestModel pullRequest);

        /// <summary>
        /// Gets the history divergence between the current HEAD and the specified pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        /// <returns></returns>
        IObservable<BranchTrackingDetails> CalculateHistoryDivergence(ILocalRepositoryModel repository, int pullRequestNumber);

        /// <summary>
        /// Gets the changes between the pull request base and head.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <returns></returns>
        IObservable<TreeChanges> GetTreeChanges(ILocalRepositoryModel repository, IPullRequestModel pullRequest);

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
        /// config by <see cref="Checkout(ILocalRepositoryModel, IPullRequestModel, string)"/>.
        /// </remarks>
        IObservable<Tuple<string, int>> GetPullRequestForCurrentBranch(ILocalRepositoryModel repository);

        /// <summary>
        /// Gets the encoding for the specified file.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="relativePath">The relative path to the file in the repository.</param>
        /// <returns>
        /// The file's encoding or <see cref="Encoding.Default"/> if the file doesn't exist.
        /// </returns>
        Encoding GetEncoding(ILocalRepositoryModel repository, string relativePath);

        /// <summary>
        /// Gets a file as it appears in a pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <param name="fileName">The filename relative to the repository root.</param>
        /// <param name="head">If true, gets the file at the PR head, otherwise gets the file at the PR base.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>The paths of the left and right files for the diff.</returns>
        IObservable<string> ExtractFile(
            ILocalRepositoryModel repository,
            IPullRequestModel pullRequest,
            string fileName,
            bool head,
            Encoding encoding);

        /// <summary>
        /// Remotes all unused remotes that were created by GitHub for Visual Studio to track PRs
        /// from forks.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        IObservable<Unit> RemoveUnusedRemotes(ILocalRepositoryModel repository);

        IObservable<string> GetPullRequestTemplate(ILocalRepositoryModel repository);

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
            ILocalRepositoryModel repository,
            string baseBranch,
            string compareBranch,
            int maxCommits);
    }
}
