using System.ComponentModel.Composition;
using GitHub.Models;

namespace GitHub.TeamFoundation.Services
{
    class LocalRepositoryModelFactory : ILocalRepositoryModelFactory
    {
        public ILocalRepositoryModel Create(string localPath)
        {
            return new LocalRepositoryModel(localPath);
        }
    }
}
