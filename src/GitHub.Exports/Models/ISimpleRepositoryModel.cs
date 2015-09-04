using GitHub.Primitives;
using GitHub.UI;

namespace GitHub.Models
{
    public interface ISimpleRepositoryModel
    {
        string Name { get; }
        UriString CloneUrl { get; }
        string LocalPath { get; }
        Octicon Icon { get; }

        void SetIcon(bool isPrivate, bool isFork);
    }
}
