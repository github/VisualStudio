using System;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IEnterpriseProbe
    {
        IObservable<EnterpriseProbeResult> Probe(Uri enterpriseBaseUrl);
        Task<EnterpriseProbeResult> AsyncProbe(Uri enterpriseBaseUrl);
    }
}