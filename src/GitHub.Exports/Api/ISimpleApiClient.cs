using System.Threading.Tasks;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Api
{
    public interface ISimpleApiClient
    {
        HostAddress HostAddress { get; }
        UriString OriginalUrl { get; }
        IGitHubClient GitHubClient { get; }
        Task<Repository> GetRepository();
        bool HasWiki();
        bool IsEnterprise();
        bool IsAuthenticated();
    }
}
