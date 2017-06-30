using GitHub.Api;
using GitHub.Primitives;
using System;
using System.Threading.Tasks;
using Octokit;

namespace GitHub.Factories
{
    public interface IApiClientFactory
    {
        Task<IGitHubClient> CreateGitHubClient(HostAddress hostAddress);

        Task<IApiClient> Create(HostAddress hostAddress);
    }
}
