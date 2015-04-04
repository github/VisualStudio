using GitHub.Primitives;
using GitHub.UI;
using Octokit;

namespace GitHub.Models
{
    public class RepositoryModel : IRepositoryModel
    {
        public RepositoryModel(Repository apiRepository, IAccount ownerAccount)
        {
            Name = apiRepository.Name;
            Owner = ownerAccount;
            CloneUrl = apiRepository.CloneUrl;
            Icon = apiRepository.Private
                ? Octicon.@lock
                : apiRepository.Fork
                    ? Octicon.repo_forked
                    : Octicon.repo;
        }

        public string Name { get; private set; }

        public IAccount Owner { get; private set; }

        public UriString CloneUrl { get; private set; }

        public Octicon Icon { get; private set; }
    }
}
