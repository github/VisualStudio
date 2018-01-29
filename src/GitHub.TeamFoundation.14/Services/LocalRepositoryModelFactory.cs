using System.ComponentModel.Composition;
using GitHub.Models;

namespace GitHub.TeamFoundation.Services
{
    [Export(typeof(ILocalRepositoryModelFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class LocalRepositoryModelFactory : ILocalRepositoryModelFactory
    {
        public ILocalRepositoryModel Create(string localPath)
        {
            return new LocalRepositoryModel(localPath);
        }
    }
}
