using GitHub.Primitives;
using GitHub.UI;

namespace GitHub.Models
{
    public interface IRepositoryModel
    {
        IAccount Owner { get; }
        string Name { get; }
        UriString CloneUrl { get; }
        Octicon Icon { get; }
    }
}
