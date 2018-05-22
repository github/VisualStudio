using System;
using System.Reactive;
using GitHub.Models;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Api
{
    public interface IApiClient
    {
        HostAddress HostAddress { get; }

        // HACK: This is temporary. Should be removed in login refactor timeframe.
        IGitHubClient GitHubClient { get; }

        IObservable<Repository> CreateRepository(NewRepository repository, string login, bool isUser);
        IObservable<Repository> ForkRepository(string owner, string name, NewRepositoryFork repository);
        IObservable<Gist> CreateGist(NewGist newGist);
        IObservable<User> GetUser();
        IObservable<User> GetUser(string login);
        IObservable<Organization> GetOrganizations();
        /// <summary>
        /// Retrieves all repositories that belong to this user.
        /// </summary>
        /// <returns></returns>
        IObservable<Repository> GetUserRepositories(RepositoryType repositoryType);
        /// <summary>
        /// Retrieves the repository for this org.
        /// </summary>
        /// <returns></returns>
        IObservable<Repository> GetRepositoriesForOrganization(string organization);

        IObservable<Repository> GetForks(string owner, string name);
        IObservable<string> GetGitIgnoreTemplates();
        IObservable<LicenseMetadata> GetLicenses();
        IObservable<Unit> DeleteApplicationAuthorization(int id, string twoFactorAuthorizationCode);
        IObservable<IssueComment> GetIssueComments(string owner, string name, int number);
        IObservable<PullRequest> GetPullRequest(string owner, string name, int number);
        IObservable<PullRequestFile> GetPullRequestFiles(string owner, string name, int number);
        IObservable<PullRequestReviewComment> GetPullRequestReviewComments(string owner, string name, int number);
        IObservable<PullRequest> GetPullRequestsForRepository(string owner, string name);
        IObservable<PullRequest> CreatePullRequest(NewPullRequest pullRequest, string owner, string repo);

        /// <summary>
        /// Posts a new PR review.
        /// </summary>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="number">The pull request number.</param>
        /// <param name="commitId">The SHA of the commit being reviewed.</param>
        /// <param name="body">The review body.</param>
        /// <param name="e">The review event.</param>
        /// <returns></returns>
        IObservable<PullRequestReview> PostPullRequestReview(
            string owner,
            string name,
            int number,
            string commitId,
            string body,
            PullRequestReviewEvent e);

        /// <summary>
        /// Creates a new PR review comment.
        /// </summary>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="number">The pull request number.</param>
        /// <param name="body">The comment body.</param>
        /// <param name="commitId">THe SHA of the commit to comment on.</param>
        /// <param name="path">The relative path of the file to comment on.</param>
        /// <param name="position">The line index in the diff to comment on.</param>
        /// <returns></returns>
        IObservable<PullRequestReviewComment> CreatePullRequestReviewComment(
            string owner,
            string name,
            int number,
            string body,
            string commitId,
            string path,
            int position);

        /// <summary>
        /// Creates a new PR review comment reply.
        /// </summary>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The repository name.</param>
        /// <param name="number">The pull request number.</param>
        /// <param name="body">The comment body.</param>
        /// <param name="inReplyTo">The comment ID to reply to.</param>
        /// <returns></returns>
        IObservable<PullRequestReviewComment> CreatePullRequestReviewComment(string owner, string name, int number, string body, int inReplyTo);

        IObservable<Branch> GetBranches(string owner, string repo);
        IObservable<Repository> GetRepositories();
        IObservable<Repository> GetRepository(string owner, string repo);
        IObservable<RepositoryContent> GetFileContents(string owner, string name, string reference, string path);

        IObservable<Unit> DeletePullRequestReviewComment(
            string owner,
            string name,
            int number);
    }
}
