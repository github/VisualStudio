using GitHub.Services;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Api
{
    public interface ISimpleApiClient
    {
        HostAddress HostAddress { get; }
        Uri OriginalUrl { get; }
        Task<EnterpriseProbeResult> IsEnterprise();
        Task<Repository> GetRepository();
    }
}
