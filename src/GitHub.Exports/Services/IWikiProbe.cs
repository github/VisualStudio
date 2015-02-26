using System.Threading.Tasks;
using Octokit;

namespace GitHub.Services
{
    public interface IWikiProbe
    {
        Task<WikiProbeResult> ProbeAsync(Repository repo);
    }
}
