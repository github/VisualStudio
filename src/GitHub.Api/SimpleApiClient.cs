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
        readonly Lazy<IEnterpriseProbe> lazyEnterpriseProbe;

        Repository repositoryCache;

        internal SimpleApiClient(HostAddress hostAddress, Uri repoUrl, GitHubClient githubClient, Lazy<IEnterpriseProbe> enterpriseProbe)
        {
            HostAddress = hostAddress;
            client = githubClient;
            lazyEnterpriseProbe = enterpriseProbe;
            OriginalUrl = repoUrl;
        }

        public async Task<Repository> GetRepository()
        {
            if (repositoryCache == null)
            {
                string owner = OriginalUrl.GetUser();
                string name = OriginalUrl.GetRepo();
                try {
                    repositoryCache = await client.Repository.Get(owner, name);
                } catch // TODO: if the repo is private, then it'll throw
                {
                }
            }
            return repositoryCache;
        }

        public async Task<EnterpriseProbeResult> IsEnterprise()
        {
            var enterpriseProbe = lazyEnterpriseProbe.Value;
            return await enterpriseProbe.AsyncProbe(HostAddress.WebUri);

        }
    }
}
