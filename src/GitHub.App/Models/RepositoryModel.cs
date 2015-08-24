using GitHub.Primitives;

namespace GitHub.Models
{
    public class RepositoryModel : SimpleRepositoryModel, IRepositoryModel
    {
        public RepositoryModel(string name, UriString cloneUrl, bool isPrivate, bool isFork,  IAccount ownerAccount)
            : base(name, cloneUrl)
        {
            Owner = ownerAccount;
            SetIcon(isPrivate, isFork);
        }

        public IAccount Owner { get; private set; }
    }
}
