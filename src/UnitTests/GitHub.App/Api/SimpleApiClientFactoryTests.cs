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
            var program = new Program();
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var factory = new SimpleApiClientFactory(
                program,
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var client = factory.Create("https://github.com/github/visualstudio");

            Assert.Equal("https://github.com/github/visualstudio", client.OriginalUrl);
            Assert.Equal(HostAddress.GitHubDotComHostAddress, client.HostAddress);
            Assert.Same(client, factory.Create("https://github.com/github/visualstudio")); // Tests caching.
        }
    }

    public class TheClearFromCacheMethod
    {
        [Fact]
        public void RemovesClientFromCache()
        {
            var program = new Program();
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var factory = new SimpleApiClientFactory(
                program,
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var client = factory.Create("https://github.com/github/visualstudio");
            factory.ClearFromCache(client);

            Assert.NotSame(client, factory.Create("https://github.com/github/visualstudio"));
        }
    }
}
