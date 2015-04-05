using System.Reactive.Linq;
using Akavache;
using GitHub.Api;
using GitHub.Caches;
using NSubstitute;

namespace UnitTests.Helpers
{
    public class TestHostCache : HostCache
    {
        public TestHostCache() : base(new InMemoryBlobCache(), CreateFakeApiClient())
        {
        }

        static IApiClient CreateFakeApiClient()
        {
            var apiClient = Substitute.For<IApiClient>();
            apiClient.GetUser().Returns(Observable.Empty<Octokit.User>());
            apiClient.GetOrganizations().Returns(Observable.Empty<Octokit.Organization>());
            return apiClient;
        }
    }
}
