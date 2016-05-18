using Akavache;
using GitHub.Primitives;
using System;

namespace GitHub.Factories
{
    public interface IHostCacheFactory : IDisposable
    {
        IBlobCache Create(HostAddress hostAddress);
    }
}
