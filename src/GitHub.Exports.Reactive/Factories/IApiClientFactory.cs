using GitHub.Api;
using GitHub.Primitives;
using System;
using System.Threading.Tasks;

namespace GitHub.Factories
{
    public interface IApiClientFactory
    {
        Task<IApiClient> Create(HostAddress hostAddress);
    }
}
