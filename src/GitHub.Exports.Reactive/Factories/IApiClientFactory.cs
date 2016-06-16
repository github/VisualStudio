using GitHub.Api;
using GitHub.Primitives;
using System;

namespace GitHub.Factories
{
    public interface IApiClientFactory
    {
        IApiClient Create(HostAddress hostAddress);
    }
}
