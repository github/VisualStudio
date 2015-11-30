using System.Threading.Tasks;
using Octokit;

namespace GitHub.Services
{
    public interface IGistCreator
    {
        Task<Gist> CreateFromSelectedText();
    }
}
