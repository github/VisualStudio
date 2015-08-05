using GitHub.Primitives;
using GitHub.UI;
using System;

namespace GitHub.Models
{
    public class RepositoryModel : SimpleRepositoryModel, IRepositoryModel
    {
        public RepositoryModel(string name, Uri cloneUrl, bool isPrivate, bool isFork,  IAccount ownerAccount)
            : base(name, cloneUrl)
        {
            Owner = ownerAccount;
            Icon = isPrivate
                ? Octicon.@lock
                : isFork
                    ? Octicon.repo_forked
                    : Octicon.repo;
        }

        public IAccount Owner { get; private set; }
        public Octicon Icon { get; private set; }
    }
}
