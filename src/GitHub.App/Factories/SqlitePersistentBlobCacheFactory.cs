using System.ComponentModel.Composition;
using Akavache;

namespace GitHub.Factories
{
    [Export(typeof(IBlobCacheFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SqlitePersistentBlobCacheFactory : IBlobCacheFactory
    {
        public IBlobCache CreateBlobCache(string path)
        {
            Guard.ArgumentNotEmptyString(path, "path");

            return new InMemoryBlobCache();
            //return new SQLitePersistentBlobCache(path);
        }
    }
}