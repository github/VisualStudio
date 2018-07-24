using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Primitives;

namespace GitHub.Services
{
    public interface IChecksService
    {
        Task<List<GitHub.Models.CheckSuiteModel>> ReadCheckSuites(
            HostAddress address,
            string owner,
            string name,
            int pullRequestNumber);
    }
}