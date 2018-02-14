using System;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using NSubstitute;
using Octokit;
using NUnit.Framework;

public class SimpleApiClientFactoryTests
{
    public class TheCreateMethod
    {
        [Test]
        public async Task CreatesNewInstanceOfSimpleApiClient()
        {
            const string url = "https://github.com/github/CreatesNewInstanceOfSimpleApiClient";
            var program = new Program();
            var keychain = Substitute.For<IKeychain>();
            var enterpriseProbe = Substitute.For<IEnterpriseProbe>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var factory = new SimpleApiClientFactory(
                program,
                CreateKeychain(),
                new Lazy<IEnterpriseProbe>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var client = await factory.Create(url);

            Assert.That(url, Is.EqualTo(client.OriginalUrl));
            Assert.That(HostAddress.GitHubDotComHostAddress, Is.EqualTo(client.HostAddress));
            Assert.That(client,Is.SameAs(await factory.Create(url))); // Tests caching.
        }
    }

    public class TheClearFromCacheMethod
    {
        [Test]
        public async Task RemovesClientFromCache()
        {
            const string url = "https://github.com/github/RemovesClientFromCache";
            var program = new Program();
            var enterpriseProbe = Substitute.For<IEnterpriseProbe>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var factory = new SimpleApiClientFactory(
                program,
                CreateKeychain(),
                new Lazy<IEnterpriseProbe>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var client = await factory.Create(url);
            factory.ClearFromCache(client);

            Assert.That(client, Is.Not.SameAs(factory.Create(url)));
        }
    }

    static IKeychain CreateKeychain()
    {
        var result = Substitute.For<IKeychain>();
        result.Load(null).ReturnsForAnyArgs(Tuple.Create("user", "pass"));
        return result;
    }
}
