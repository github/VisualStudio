using Akavache;
using System;

namespace GitHub.Factories
{
    public interface IBlobCacheFactory : IDisposable
    {
        IBlobCache CreateBlobCache(string path);
    }
}
