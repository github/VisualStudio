using System;
using System.Reactive;
using GitHub.Models;

namespace GitHub.Services
{
    public interface IPullRequestService
    {
        IObservable<IPullRequestModel> CreatePullRequest(IRepositoryHost host,
            ILocalRepositoryModel sourceRepository, IRepositoryModel targetRepository,
            IBranch sourceBranch, IBranch targetBranch,
            string title, string body);

        /// <summary>
        /// Fetches a pull request to a local branch and checks out the branch.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequestNumber">The number of the pull request.</param>
        /// <param name="localBranchName">The name of the local branch.</param>
        /// <returns></returns>
        IObservable<Unit> FetchAndCheckout(ILocalRepositoryModel repository, int pullRequestNumber, string localBranchName);

        /// <summary>
        /// Gets the name of the local branch that a call to <see cref="Checkout(ILocalRepositoryModel, IPullRequestModel)"/>
        /// will check out to.
        /// </summary>
        /// <param name="pullRequestNumber">The pull request number.</param>
        /// <param name="pullRequestTitle">The pull request title.</param>
        /// <returns></returns>
        string GetDefaultLocalBranchName(int pullRequestNumber, string pullRequestTitle);

        /// <summary>
        /// Gets the local branches that exist for the specified pull request.
        /// </summary>
        /// <param name="pullRequestNumber">The pull request number.</param>
        /// <returns></returns>
        IObservable<IBranch> GetLocalBranches(ILocalRepositoryModel repository, int number);

        /// <summary>
        /// Switches to an existing branch for the specified pull request.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="pullRequestNumber">The pull request number.</param>
        /// <returns></returns>
        IObservable<Unit> SwitchToBranch(ILocalRepositoryModel repository, int number);

        IObservable<string> GetPullRequestTemplate(ILocalRepositoryModel repository);
    }
}
