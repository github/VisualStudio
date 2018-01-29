using System.Threading.Tasks;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Api
{
    public interface ISimpleApiClient
    {
        IGitHubClient Client { get; }
        HostAddress HostAddress { get; }
        UriString OriginalUrl { get; }
        Task<Repository> GetRepository();
        bool HasWiki();
        bool IsEnterprise();
        bool IsAuthenticated();
    }
}
