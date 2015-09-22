using System;

namespace GitHub.Services
{
    public interface IEnterpriseProbe
    {
        IObservable<EnterpriseProbeResult> Probe(Uri enterpriseBaseUrl);
    }
}
