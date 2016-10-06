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
        /// Checks whether the specified repository is in a clean state that will allow a checkout.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        IObservable<bool> IsCleanForCheckout(ILocalRepositoryModel repository);

        /// <summary>
        /// Fetches a pull request to a local branch and checks out the branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequestNumber">The number of the pull request.</param>
        /// <param name="localBranchName">The name of the local branch.</param>
        /// <returns></returns>
        IObservable<Unit> FetchAndCheckout(ILocalRepositoryModel repository, int pullRequestNumber, string localBranchName);

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
        /// <param name="pullRequest">The octokit pull request details.</param>
        /// <returns></returns>
        IObservable<IBranch> GetLocalBranches(ILocalRepositoryModel repository, PullRequest pullRequest);

        /// <summary>
        /// Determines whether the specified pull request is from a fork.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The octokit pull request details.</param>
        /// <returns></returns>
        bool IsPullRequestFromFork(ILocalRepositoryModel repository, PullRequest pullRequest);

        /// <summary>
        /// Switches to an existing branch for the specified pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequest">The octokit pull request details.</param>
        /// <returns></returns>
        IObservable<Unit> SwitchToBranch(ILocalRepositoryModel repository, PullRequest pullRequest);

        /// <summary>
        /// Gets the history divergence between the current HEAD and the specified pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        /// <returns></returns>
        IObservable<HistoryDivergence> CalculateHistoryDivergence(ILocalRepositoryModel repository, int pullRequestNumber);

        /// <summary>
        /// Removes any association between the current branch and a pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        IObservable<Unit> UnmarkLocalBranch(ILocalRepositoryModel repository);

        IObservable<string> GetPullRequestTemplate(ILocalRepositoryModel repository);
    }
}
