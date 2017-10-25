using System.Threading.Tasks;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Api
{
    public interface ISimpleApiClientFactory
    {
        Task<ISimpleApiClient> Create(UriString repositoryUrl);
        Task<IGitHubClient> CreateGitHubClient(HostAddress address);
        void ClearFromCache(ISimpleApiClient client);
    }
}
