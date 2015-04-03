using GitHub.Api;
using GitHub.Caches;
using GitHub.Primitives;

namespace GitHub.Factories
{
    public interface IHostCacheFactory
    {
        IHostCache Create(HostAddress hostAddress, IApiClient apiClient);
    }
}
