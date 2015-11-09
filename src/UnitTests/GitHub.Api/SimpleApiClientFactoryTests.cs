using System;
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
        public void CreatesNewInstanceOfSimpleApiClient()
        {
            const string url = "https://github.com/github/CreatesNewInstanceOfSimpleApiClient";
            var program = new Program();
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var factory = new SimpleApiClientFactory(
                program,
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var client = factory.Create(url);

            Assert.Equal(url, client.OriginalUrl);
            Assert.Equal(HostAddress.GitHubDotComHostAddress, client.HostAddress);
            Assert.Same(client, factory.Create(url)); // Tests caching.
        }
    }

    public class TheClearFromCacheMethod
    {
        [Fact]
        public void RemovesClientFromCache()
        {
            const string url = "https://github.com/github/RemovesClientFromCache";
            var program = new Program();
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var factory = new SimpleApiClientFactory(
                program,
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var client = factory.Create(url);
            factory.ClearFromCache(client);

            Assert.NotSame(client, factory.Create(url));
        }
    }
}
