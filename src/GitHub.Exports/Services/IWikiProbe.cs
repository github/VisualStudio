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
        Task<WikiProbeResult> ProbeAsync(Repository repo);
    }
}
