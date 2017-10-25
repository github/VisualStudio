using System;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public enum EnterpriseProbeResult
    {
        /// <summary>
        /// Yep! It's an Enterprise server
        /// </summary>
        Ok,

        /// <summary>
        /// Got a response from a server, but it wasn't an Enterprise server
        /// </summary>
        NotFound,

        /// <summary>
        /// Request timed out or DNS failed. So it's probably the case it's not an enterprise server but 
        /// we can't know for sure.
        /// </summary>
        Failed
    }

    public interface IEnterpriseProbeTask
    {
        Task<EnterpriseProbeResult> ProbeAsync(Uri enterpriseBaseUrl);
    }
}
