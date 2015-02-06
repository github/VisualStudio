using GitHub.Extensions;
using Octokit;
using System;
using System.Threading.Tasks;

namespace GitHub.Api
{
    public class SimpleApiClient : ISimpleApiClient
    {
        public HostAddress HostAddress { get; private set; }
        readonly GitHubClient client;
        readonly Uri repoUrl;

        internal SimpleApiClient(HostAddress hostAddress, Uri repoUrl, GitHubClient githubClient)
        {
            HostAddress = hostAddress;
            client = githubClient;
            this.repoUrl = repoUrl;
        }

        public async Task<Repository> GetRepository()
        {
            string owner = repoUrl.GetUser();
            string name = repoUrl.GetRepo();
            return await client.Repository.Get(owner, name);
        }
    }
}
