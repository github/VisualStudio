using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Services;
using Octokit;
using System;
using System.Threading.Tasks;


namespace GitHub.Api
{
    public class SimpleApiClient : ISimpleApiClient
    {
        public HostAddress HostAddress { get; private set; }
        public Uri OriginalUrl { get; private set; }


        readonly GitHubClient client;
        readonly Lazy<IEnterpriseProbe> enterpriseProbe;
        readonly Lazy<IWikiProbe> wikiProbe;

        Repository repositoryCache;
        string owner;
        string repoName;

        internal SimpleApiClient(HostAddress hostAddress, Uri repoUrl, GitHubClient githubClient,
            Lazy<IEnterpriseProbe> enterpriseProbe, Lazy<IWikiProbe> wikiProbe)
        {
            this.HostAddress = hostAddress;
            this.client = githubClient;
            this.enterpriseProbe = enterpriseProbe;
            this.wikiProbe = wikiProbe;
            this.OriginalUrl = repoUrl;
        }

        public async Task<Repository> GetRepository()
        {
            if (owner == null)
            {
                owner = OriginalUrl.GetUser();
                repoName = OriginalUrl.GetRepo();

                try
                {
                    repositoryCache = await client.Repository.Get(owner, repoName);
                }
                catch // TODO: if the repo is private, then it'll throw
                {
                }
            }
            return repositoryCache;
        }

        public async Task<WikiProbeResult> HasWiki()
        {
            var repo = await GetRepository();
            if (repo == null || !repo.HasWiki)
                return WikiProbeResult.NotFound;

            var probe = wikiProbe.Value;
            return await probe.AsyncProbe(repo);
        }

        public async Task<EnterpriseProbeResult> IsEnterprise()
        {
            var probe = enterpriseProbe.Value;
            return await probe.AsyncProbe(HostAddress.WebUri);
        }
    }
}
