using Akavache;
using GitHub.Caches;

namespace UnitTests.Helpers
{
    public class TestSharedCache : SharedCache
    {
        public TestSharedCache() : base(new InMemoryBlobCache(), new InMemoryBlobCache())
        {
        }
    }
}
