using Akavache;
using GitHub.Primitives;
using System;
using System.Threading.Tasks;

namespace GitHub.Factories
{
    public interface IHostCacheFactory : IDisposable
    {
        Task<IBlobCache> Create(HostAddress hostAddress);
    }
}
