using System.Threading.Tasks;
using Octokit;

namespace GitHub.Services
{
    public interface IGistCreator
    {
        Task<Gist> CreateGist(string fileName, bool isPublic, string content = "", string description = "");
    }
}
