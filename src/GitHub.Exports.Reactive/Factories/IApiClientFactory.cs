using GitHub.Api;
using GitHub.Primitives;
using System;

namespace GitHub.Factories
{
    public interface IApiClientFactory : IDisposable
    {
        IApiClient Create(HostAddress hostAddress);
    }
}
