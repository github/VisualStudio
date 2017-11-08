using System;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using NSubstitute;
using Xunit;

public class SimpleApiClientFactoryTests
{
    public class TheCreateMethod
    {
        [Fact]
        public async Task CreatesNewInstanceOfSimpleApiClient()
        {
            const string url = "https://github.com/github/CreatesNewInstanceOfSimpleApiClient";
            var program = new Program();
            var keychain = Substitute.For<IKeychain>();
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var factory = new SimpleApiClientFactory(
                program,
                CreateKeychain(),
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var client = await factory.Create(url);

            Assert.Equal(url, client.OriginalUrl);
            Assert.Equal(HostAddress.GitHubDotComHostAddress, client.HostAddress);
            Assert.Same(client, await factory.Create(url)); // Tests caching.
        }
    }

    public class TheClearFromCacheMethod
    {
        [Fact]
        public async Task RemovesClientFromCache()
        {
            const string url = "https://github.com/github/RemovesClientFromCache";
            var program = new Program();
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var factory = new SimpleApiClientFactory(
                program,
                CreateKeychain(),
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var client = await factory.Create(url);
            factory.ClearFromCache(client);

            Assert.NotSame(client, factory.Create(url));
        }
    }

    static IKeychain CreateKeychain()
    {
        var result = Substitute.For<IKeychain>();
        result.Load(null).ReturnsForAnyArgs(Tuple.Create("user", "pass"));
        return result;
    }
}
