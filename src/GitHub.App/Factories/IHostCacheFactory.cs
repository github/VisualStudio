using Akavache;
using GitHub.Primitives;

namespace GitHub.Factories
{
    public interface IHostCacheFactory
    {
        IBlobCache Create(HostAddress hostAddress);
    }
}
