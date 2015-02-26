using System;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IEnterpriseProbeTask
    {
        Task<EnterpriseProbeResult> ProbeAsync(Uri enterpriseBaseUrl);
    }
}
