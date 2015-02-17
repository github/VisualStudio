using System;
using System.ComponentModel.Composition;
using GitHub.Models;
using Octokit.Internal;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace GitHub.Services
{
    [Export(typeof(IEnterpriseProbe))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class EnterpriseProbe : EnterpriseProbeTask, IEnterpriseProbe
    {
        [ImportingConstructor]
        public EnterpriseProbe(IProgram program, IHttpClient httpClient)
            : base(program, httpClient)
        {
        }

        public IObservable<EnterpriseProbeResult> Probe(Uri enterpriseBaseUrl)
        {
            return ProbeAsync(enterpriseBaseUrl).ToObservable();
        }
    }
}
