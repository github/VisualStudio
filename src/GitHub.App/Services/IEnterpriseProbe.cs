using System;
using GitHub.Helpers;

namespace GitHub.Services
{
    public interface IEnterpriseProbe
    {
        IObservable<EnterpriseProbeResult> Probe(Uri enterpriseBaseUrl);
    }
}