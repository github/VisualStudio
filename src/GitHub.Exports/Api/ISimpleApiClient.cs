using System;
using System.Threading.Tasks;
using GitHub.Primitives;
using GitHub.Services;
using Octokit;

namespace GitHub.Api
{
    public interface ISimpleApiClient
    {
        HostAddress HostAddress { get; }
        Uri OriginalUrl { get; }
        Task<EnterpriseProbeResult> IsEnterprise();
        Task<Repository> GetRepository();
        Task<WikiProbeResult> HasWiki();
    }
}
