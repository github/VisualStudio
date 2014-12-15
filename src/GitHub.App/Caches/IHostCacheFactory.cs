using System;

namespace GitHub
{
    public interface IHostCacheFactory
    {
        IHostCache Create(HostAddress hostAddress);
    }
}
