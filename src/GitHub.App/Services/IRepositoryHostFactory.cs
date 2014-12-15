using GitHub.Models;

namespace GitHub
{
    public interface IRepositoryHostFactory
    {
        IRepositoryHost Create(HostAddress hostAddress);
    }
}