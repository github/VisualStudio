using GitHub.Models;
using GitHub.Services;

namespace GitHub.TeamFoundation.Services
{
    class LocalRepositoryModelFactory : ILocalRepositoryModelFactory
    {
        public ILocalRepositoryModel Create(string localPath)
        {
            return GitService.GitServiceHelper.CreateLocalRepositoryModel(localPath);
        }
    }
}
