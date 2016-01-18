using Akavache;
using Akavache.Sqlite3;
using NLog;
using System;
using System.ComponentModel.Composition;

namespace GitHub.Factories
{
    [Export(typeof(IBlobCacheFactory))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SqlitePersistentBlobCacheFactory : IBlobCacheFactory
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        public IBlobCache CreateBlobCache(string path)
        {
            Guard.ArgumentNotEmptyString(path, "path");

            try
            {
                return new SQLitePersistentBlobCache(path);
            }
            catch(Exception ex)
            {
                log.Error("Error while creating SQLitePersistentBlobCache for {0}. {1}", path, ex);
                return new InMemoryBlobCache();
            }
        }
    }
}