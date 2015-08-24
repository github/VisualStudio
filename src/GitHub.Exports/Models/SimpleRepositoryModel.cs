using GitHub.Primitives;
using GitHub.UI;
using GitHub.VisualStudio.Helpers;

namespace GitHub.Models
{
    public class SimpleRepositoryModel : NotificationAwareObject, ISimpleRepositoryModel, INotifyPropertySource
    {
        public SimpleRepositoryModel(string name, UriString cloneUrl, string localPath = null)
        {
            Name = name;
            CloneUrl = cloneUrl;
            LocalPath = localPath;
            Icon = Octicon.repo;
        }

        public void SetIcon(bool isPrivate, bool isFork)
        {
            Icon = isPrivate
                    ? Octicon.@lock
                    : isFork
                        ? Octicon.repo_forked
                        : Octicon.repo;
        }

        public string Name { get; private set; }
        public UriString CloneUrl { get; private set; }
        public string LocalPath { get; private set; }
        Octicon icon;
        public Octicon Icon { get { return icon; } set { icon = value; this.RaisePropertyChange(); } }

    }
}
