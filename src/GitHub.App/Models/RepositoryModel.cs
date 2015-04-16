using GitHub.Primitives;
using GitHub.UI;
using Octokit;

namespace GitHub.Models
{
    public class RepositoryModel : IRepositoryModel
    {
        public RepositoryModel(string name, string cloneUrl, bool isPrivate, bool isFork,  IAccount ownerAccount)
        {
            Name = name;
            Owner = ownerAccount;
            CloneUrl = cloneUrl;
            Icon = isPrivate
                ? Octicon.@lock
                : isFork
                    ? Octicon.repo_forked
                    : Octicon.repo;
        }

        public string Name { get; private set; }

        public IAccount Owner { get; private set; }

        public UriString CloneUrl { get; private set; }

        public Octicon Icon { get; private set; }
    }
}
