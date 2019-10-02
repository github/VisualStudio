using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using LibGit2Sharp;

namespace GitHub.SampleData
{
    class GitServiceDesigner : IGitService
    {
        public LocalRepositoryModel CreateLocalRepositoryModel(string localPath) => null;
        public BranchModel GetBranch(LocalRepositoryModel model) => null;
        public Task<string> GetLatestPushedSha(string path, string remote = "origin") => Task.FromResult<string>(null);
        public UriString GetRemoteUri(IRepository repo, string remote = "origin") => null;
        public IRepository GetRepository(string path) => null;
        public UriString GetUri(string path, string remote = "origin") => null;
        public UriString GetUri(IRepository repository, string remote = "origin") => null;
        public Task<Patch> Compare(IRepository repository, string sha1, string sha2, string relativePath) => null;
        public Task<ContentChanges> CompareWith(IRepository repository, string sha1, string sha2, string path, byte[] contents) => null;
        public Task<TreeChanges> Compare(IRepository repository, string sha1, string sha2, bool detectRenames = false) => null;
    }
}
