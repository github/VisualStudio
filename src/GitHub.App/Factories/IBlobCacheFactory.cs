using Akavache;

namespace GitHub.Factories
{
    public interface IBlobCacheFactory
    {
        IBlobCache CreateBlobCache(string path);
    }
}
