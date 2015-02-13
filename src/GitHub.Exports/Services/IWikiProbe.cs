using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IWikiProbe
    {
        IObservable<WikiProbeResult> Probe(Repository repo);
        Task<WikiProbeResult> AsyncProbe(Repository repo);
    }
}
