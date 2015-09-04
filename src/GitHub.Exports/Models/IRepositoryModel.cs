using GitHub.Primitives;
using GitHub.UI;

namespace GitHub.Models
{
    public interface IRepositoryModel : ISimpleRepositoryModel
    {
        IAccount Owner { get; }
    }
}
