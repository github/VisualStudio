using Akavache;

namespace GitHub
{
    public interface IBlobCacheFactory
    {
        IBlobCache CreateBlobCache(string path);
    }
}
