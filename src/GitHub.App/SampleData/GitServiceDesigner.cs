using System.Threading.Tasks;
using GitHub.Primitives;
using GitHub.Services;
using LibGit2Sharp;

namespace GitHub.SampleData
{
    class GitServiceDesigner : IGitService
    {
        public Task<string> GetLatestPushedSha(string path) => Task.FromResult<string>(null);
        public UriString GetRemoteUri(IRepository repo, string remote = null) => null;
        public IRepository GetRepository(string path) => null;
        public UriString GetUri(string path, string remote = null) => null;
        public UriString GetUri(IRepository repository, string remote = null) => null;
        public string FindOriginalRemoteName(IRepository repo) => null;
    }
}
