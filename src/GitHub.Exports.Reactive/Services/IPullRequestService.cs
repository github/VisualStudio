using System;
using System.Reactive;
using GitHub.Models;
using LibGit2Sharp;
using Octokit;

namespace GitHub.Services
{
    public interface IPullRequestService
    {
        IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host,
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
        /// An observable that produces a single value: the pull request number, or 0 if the
        /// current branch is not a PR branch.
        /// </returns>
        /// <remarks>
        /// This method does not do an API request - it simply checks the mark left in the git
        /// config by <see cref="Checkout(ILocalRepositoryModel, IPullRequestModel, string)"/>.
        /// </remarks>
        IObservable<int> GetPullRequestForCurrentBranch(ILocalRepositoryModel repository);

        /// <summary>
        /// Removes any association between the current branch and a pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        IObservable<Unit> UnmarkLocalBranch(ILocalRepositoryModel repository);

        /// <summary>
        /// Gets the left and right files for a diff.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="modelService">A model service to use as a cache if the file is not fetched.</param>
        /// <param name="pullRequest">The pull request details.</param>
        /// <param name="fileName">The filename relative to the repository root.</param>
        /// <param name="fileSha">The SHA of the file in the pull request.</param>
        /// <param name="isPullRequestBranchCheckedOut">
        /// Whether the pull request branch is currently checked out. If so the right file returned
        /// will be the path to the file in the working directory.
        /// </param>
        /// <returns>The paths of the left and right files for the diff.</returns>
        IObservable<Tuple<string, string>> ExtractDiffFiles(
            ILocalRepositoryModel repository,
            IPullRequestModel pullRequest,
            string fileName,
            bool isPullRequestBranchCheckedOut);

        /// <summary>
        /// Remotes all unused remotes that were created by GitHub for Visual Studio to track PRs
        /// from forks.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        IObservable<Unit> RemoveUnusedRemotes(ILocalRepositoryModel repository);

        IObservable<string> GetPullRequestTemplate(ILocalRepositoryModel repository);
    }
}
