using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using GitHub.Extensions;
using GitHub.Logging;
using GitHub.Primitives;
using Octokit;
using Octokit.Reactive;
using Serilog;

namespace GitHub.Api
{
    public partial class ApiClient : IApiClient
    {
        const string ScopesHeader = "X-OAuth-Scopes";
        const string ProductName = Info.ApplicationInfo.ApplicationDescription;
        static readonly ILogger log = LogManager.ForContext<ApiClient>();

        readonly IObservableGitHubClient gitHubClient;

        string ClientId { get; set; }
        string ClientSecret { get; set; }

        public ApiClient(HostAddress hostAddress, IObservableGitHubClient gitHubClient)
        {
            ClientId = ApiClientConfiguration.ClientId;
            ClientSecret = ApiClientConfiguration.ClientSecret;
            HostAddress = hostAddress;
            this.gitHubClient = gitHubClient;
        }

        partial void Configure();

        public IGitHubClient GitHubClient => new GitHubClient(gitHubClient.Connection);

        public IObservable<Repository> CreateRepository(NewRepository repository, string login, bool isUser)
        {
            Guard.ArgumentNotEmptyString(login, nameof(login));

            var client = gitHubClient.Repository;

            return (isUser ? client.Create(repository) : client.Create(login, repository));
        }

        public IObservable<PullRequestReviewComment> CreatePullRequestReviewComment(
            string owner,
            string name,
            int number,
            string body,
            string commitId,
            string path,
            int position)
        {
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(name, nameof(name));
            Guard.ArgumentNotEmptyString(body, nameof(body));
            Guard.ArgumentNotEmptyString(commitId, nameof(commitId));
            Guard.ArgumentNotEmptyString(path, nameof(path));

            var comment = new PullRequestReviewCommentCreate(body, commitId, path, position);
            return gitHubClient.PullRequest.ReviewComment.Create(owner, name, number, comment);
        }

        public IObservable<PullRequestReviewComment> CreatePullRequestReviewComment(
            string owner,
            string name,
            int number,
            string body,
            int inReplyTo)
        {
            var comment = new PullRequestReviewCommentReplyCreate(body, inReplyTo);
            return gitHubClient.PullRequest.ReviewComment.CreateReply(owner, name, number, comment);
        }

        public IObservable<Gist> CreateGist(NewGist newGist)
        {
            return gitHubClient.Gist.Create(newGist);
        }

        public IObservable<User> GetUser()
        {
            return gitHubClient.User.Current();
        }

        public IObservable<Organization> GetOrganizations()
        {
            // Organization.GetAllForCurrent doesn't return all of the information we need (we 
            // need information about the plan the organization is on in order to enable/disable
            // the "Private Repository" checkbox in the "Create Repository" dialog). To get this
            // we have to do an Organization.Get on each repository received.
            return gitHubClient.Organization
                .GetAllForCurrent()
                .Select(x => gitHubClient.Organization.Get(x.Login))
                .Merge();
        }

        public IObservable<Repository> GetUserRepositories(RepositoryType repositoryType)
        {
            var request = new RepositoryRequest
            {
                Type = repositoryType,
                Direction = SortDirection.Ascending,
                Sort = RepositorySort.FullName
            };
            return gitHubClient.Repository.GetAllForCurrent(request);
        }

        public IObservable<string> GetGitIgnoreTemplates()
        {
            return gitHubClient.Miscellaneous.GetAllGitIgnoreTemplates();
        }

        public IObservable<LicenseMetadata> GetLicenses()
        {
            return gitHubClient.Miscellaneous.GetAllLicenses();
        }

        public HostAddress HostAddress { get; }

        static string GetSha256Hash(string input)
        {
            Guard.ArgumentNotEmptyString(input, nameof(input));

            try
            {
                using (var sha256 = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(input);
                    var hash = sha256.ComputeHash(bytes);

                    return string.Join("", hash.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
                }
            }
            catch (Exception e)
            {
                log.Error(e, "IMPOSSIBLE! Generating Sha256 hash caused an exception");
                return null;
            }
        }

        static string GetFingerprint()
        {
            return GetSha256Hash(ProductName + ":" + GetMachineIdentifier());
        }

        static string GetMachineNameSafe()
        {
            try
            {
                return Dns.GetHostName();
            }
            catch (Exception e)
            {
                log.Warning(e, "Failed to retrieve host name using `DNS.GetHostName`");
                try
                {
                    return Environment.MachineName;
                }
                catch (Exception ex)
                {
                    log.Warning(ex, "Failed to retrieve host name using `Environment.MachineName`");
                    return "(unknown)";
                }
            }
        }

        static string GetMachineIdentifier()
        {
            try
            {
                // adapted from http://stackoverflow.com/a/1561067
                var fastedValidNetworkInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .OrderBy(nic => nic.Speed)
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                    .Select(nic => nic.GetPhysicalAddress().ToString())
                    .FirstOrDefault(address => address.Length > 12);

                return fastedValidNetworkInterface ?? GetMachineNameSafe();
            }
            catch (Exception e)
            {
                log.Warning(e, "Could not retrieve MAC address. Fallback to using machine name");
                return GetMachineNameSafe();
            }
        }

        public IObservable<Repository> GetRepositoriesForOrganization(string organization)
        {
            Guard.ArgumentNotEmptyString(organization, nameof(organization));

            return gitHubClient.Repository.GetAllForOrg(organization);
        }

        public IObservable<Unit> DeleteApplicationAuthorization(int id, string twoFactorAuthorizationCode)
        {
            Guard.ArgumentNotEmptyString(twoFactorAuthorizationCode, nameof(twoFactorAuthorizationCode));

            return gitHubClient.Authorization.Delete(id, twoFactorAuthorizationCode);
        }

        public IObservable<IssueComment> GetIssueComments(string owner, string name, int number)
        {
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(name, nameof(name));

            return gitHubClient.Issue.Comment.GetAllForIssue(owner, name, number);
        }

        public IObservable<PullRequest> GetPullRequest(string owner, string name, int number)
        {
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(name, nameof(name));

            return gitHubClient.PullRequest.Get(owner, name, number);
        }

        public IObservable<PullRequestFile> GetPullRequestFiles(string owner, string name, int number)
        {
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(name, nameof(name));

            return gitHubClient.PullRequest.Files(owner, name, number);
        }

        public IObservable<PullRequestReviewComment> GetPullRequestReviewComments(string owner, string name, int number)
        {
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(name, nameof(name));

            return gitHubClient.PullRequest.ReviewComment.GetAll(owner, name, number);
        }

        public IObservable<PullRequest> GetPullRequestsForRepository(string owner, string name)
        {
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(name, nameof(name));

            return gitHubClient.PullRequest.GetAllForRepository(owner, name,
                new PullRequestRequest
                {
                    State = ItemStateFilter.All,
                    SortProperty = PullRequestSort.Updated,
                    SortDirection = SortDirection.Descending
                });
        }

        public IObservable<PullRequest> CreatePullRequest(NewPullRequest pullRequest, string owner, string repo)
        {
            Guard.ArgumentNotNull(pullRequest, nameof(pullRequest));
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(repo, nameof(repo));

            return gitHubClient.PullRequest.Create(owner, repo, pullRequest);
        }

        public IObservable<Repository> GetRepositories()
        {
            return gitHubClient.Repository.GetAllForCurrent();
        }

        public IObservable<Branch> GetBranches(string owner, string repo)
        {
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(repo, nameof(repo));

#pragma warning disable 618
            // GetAllBranches is obsolete, but don't want to introduce the change to fix the
            // warning in the PR, so disabling for now.
            return gitHubClient.Repository.GetAllBranches(owner, repo);
#pragma warning restore
        }

        public IObservable<Repository> GetRepository(string owner, string repo)
        {
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(repo, nameof(repo));

            return gitHubClient.Repository.Get(owner, repo);
        }

        public IObservable<RepositoryContent> GetFileContents(string owner, string name, string reference, string path)
        {
            Guard.ArgumentNotEmptyString(owner, nameof(owner));
            Guard.ArgumentNotEmptyString(name, nameof(name));
            Guard.ArgumentNotEmptyString(reference, nameof(reference));
            Guard.ArgumentNotEmptyString(path, nameof(path));

            return gitHubClient.Repository.Content.GetAllContentsByRef(owner, name, reference, path);
        }
    }
}
