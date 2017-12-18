using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;
using GitHub.Extensions;

namespace GitHub.Api
{
    public class SimpleApiClient : ISimpleApiClient
    {
        public HostAddress HostAddress { get; }
        public UriString OriginalUrl { get; }

        readonly Lazy<IEnterpriseProbe> enterpriseProbe;
        readonly Lazy<IWikiProbe> wikiProbe;
        static readonly SemaphoreSlim sem = new SemaphoreSlim(1);

        Repository repositoryCache = new Repository();
        string owner;
        bool? isEnterprise;
        bool? hasWiki;

        public SimpleApiClient(UriString repoUrl, IGitHubClient githubClient,
            Lazy<IEnterpriseProbe> enterpriseProbe, Lazy<IWikiProbe> wikiProbe)
        {
            Guard.ArgumentNotNull(repoUrl, nameof(repoUrl));
            Guard.ArgumentNotNull(githubClient, nameof(githubClient));

            HostAddress = HostAddress.Create(repoUrl);
            OriginalUrl = repoUrl;
            Client = githubClient;
            this.enterpriseProbe = enterpriseProbe;
            this.wikiProbe = wikiProbe;
        }

        public IGitHubClient Client { get; }

        public async Task<Repository> GetRepository()
        {
            // fast path to avoid locking when the cache has already been set
            // once it's been set, it's never going to be touched again, so it's safe
            // to read. This way, lock queues will only form once on first load
            if (owner != null)
                return repositoryCache;
            return await GetRepositoryInternal();
        }
        
        public bool IsAuthenticated()
        {
            // this doesn't account for auth revoke on the server but its much faster 
            // than doing the API hit.
            var authType = Client.Connection.Credentials?.AuthenticationType ?? AuthenticationType.Anonymous;
            return authType != AuthenticationType.Anonymous;
        }

        async Task<Repository> GetRepositoryInternal()
        {
            await sem.WaitAsync();
            try
            {
                if (owner == null)
                {
                    var ownerLogin = OriginalUrl.Owner;
                    var repositoryName = OriginalUrl.RepositoryName;

                    if (ownerLogin != null && repositoryName != null)
                    {
                        var repo = await Client.Repository.Get(ownerLogin, repositoryName);
                        if (repo != null)
                        {
                            hasWiki = await HasWikiInternal(repo);
                            isEnterprise = await IsEnterpriseInternal();
                            repositoryCache = repo;
                        }
                        owner = ownerLogin;
                    }
                }
            }
            // it'll throw if it's private or an enterprise instance requiring authentication
            catch (ApiException apiex)
            {
                if (!HostAddress.IsGitHubDotComUri(OriginalUrl.ToRepositoryUrl()))
                    isEnterprise = apiex.IsGitHubApiException();
            }
            catch {}
            finally
            {
                sem.Release();
            }

            return repositoryCache;
        }

        public bool HasWiki()
        {
            return hasWiki.HasValue && hasWiki.Value;
        }

        public bool IsEnterprise()
        {
            return isEnterprise.HasValue && isEnterprise.Value;
        }

        async Task<bool> HasWikiInternal(Repository repo)
        {
            if (repo == null)
                return false;

            if (!repo.HasWiki)
            {
                hasWiki = false;
                return false;
            }

            var probe = wikiProbe.Value;
            Debug.Assert(probe != null, "Lazy<Wiki> probe is not set, something is wrong.");
#if !DEBUG
            if (probe == null)
                return false;
#endif
            var ret = await probe.ProbeAsync(repo);
            return (ret == WikiProbeResult.Ok);
        }

        async Task<bool> IsEnterpriseInternal()
        {
            if (HostAddress == HostAddress.GitHubDotComHostAddress)
                return false;

            var probe = enterpriseProbe.Value;
            Debug.Assert(probe != null, "Lazy<Enterprise> probe is not set, something is wrong.");
#if !DEBUG
            if (probe == null)
                return false;
#endif
            var ret = await probe.Probe(HostAddress.WebUri);
            return (ret == EnterpriseProbeResult.Ok);
        }
    }
}
