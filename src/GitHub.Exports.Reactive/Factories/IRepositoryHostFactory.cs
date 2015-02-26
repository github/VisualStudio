using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Factories
{
    public interface IRepositoryHostFactory
    {
        IRepositoryHost Create(HostAddress hostAddress);
    }
}