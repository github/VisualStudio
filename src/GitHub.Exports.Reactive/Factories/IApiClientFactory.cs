using GitHub.Api;
using GitHub.Primitives;

namespace GitHub.Factories
{
    public interface IApiClientFactory
    {
        IApiClient Create(HostAddress hostAddress);
    }
}
