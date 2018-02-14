using System;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using GitHub.Services;
using NSubstitute;
using Octokit;
using NUnit.Framework;

public class SimpleApiClientTests
{
    public class TheCtor : TestBaseClass
    {
        public void Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SimpleApiClient(null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new SimpleApiClient("https://github.com/github/visualstudio", null, null, null));
        }
    }

    public class TheGetRepositoryMethod
    {
        [Test]
        public async Task RetrievesRepositoryFromWeb()
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var repository = new Repository(42);
            gitHubClient.Repository.Get("github", "visualstudio").Returns(Task.FromResult(repository));
            var enterpriseProbe = Substitute.For<IEnterpriseProbe>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbe>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var result = await client.GetRepository();

            Assert.That(42, Is.EqualTo(result.Id));
        }

        [Test]
        public async Task RetrievesCachedRepositoryForSubsequentCalls()
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var repository = new Repository(42);
            gitHubClient.Repository.Get("github", "visualstudio")
                .Returns(_ => Task.FromResult(repository), _ => { throw new Exception("Should only be called once."); });
            var enterpriseProbe = Substitute.For<IEnterpriseProbe>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbe>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));
            await client.GetRepository();

            var result = await client.GetRepository();

            Assert.That(42, Is.EqualTo(result.Id));
        }
    }

    public class TheHasWikiMethod
    {
        [TestCase(WikiProbeResult.Ok, true)]
        [TestCase(WikiProbeResult.Failed, false)]
        [TestCase(WikiProbeResult.NotFound, false)]
        public async Task ReturnsTrueWhenWikiProbeReturnsOk(WikiProbeResult probeResult, bool expected)
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var repository = CreateRepository(42, true);
            gitHubClient.Repository.Get("github", "visualstudio").Returns(Task.FromResult(repository));
            var enterpriseProbe = Substitute.For<IEnterpriseProbe>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            wikiProbe.ProbeAsync(repository)
                .Returns(_ => Task.FromResult(probeResult), _ => { throw new Exception("Only call it once"); });
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbe>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));
            await client.GetRepository();

            var result = client.HasWiki();

            Assert.That(expected, Is.EqualTo(result));
            Assert.That(expected, Is.EqualTo(client.HasWiki()));
        }

        [Test]
        public void ReturnsFalseWhenWeHaveNotRequestedRepository()
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var enterpriseProbe = Substitute.For<IEnterpriseProbe>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbe>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var result = client.HasWiki();

            Assert.False(result);
        }
    }

    public class TheIsEnterpriseMethod
    {
        [TestCase(EnterpriseProbeResult.Ok, true)]
        [TestCase(EnterpriseProbeResult.Failed, false)]
        [TestCase(EnterpriseProbeResult.NotFound, false)]
        public async Task ReturnsTrueWhenEnterpriseProbeReturnsOk(EnterpriseProbeResult probeResult, bool expected)
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var repository = CreateRepository(42, true);
            gitHubClient.Repository.Get("github", "visualstudio").Returns(Task.FromResult(repository));
            var enterpriseProbe = Substitute.For<IEnterpriseProbe>();
            enterpriseProbe.Probe(Args.Uri)
                .Returns(_ => Task.FromResult(probeResult), _ => { throw new Exception("Only call it once"); });
            var wikiProbe = Substitute.For<IWikiProbe>();
            var client = new SimpleApiClient(
                "https://github.enterprise/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbe>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));
            await client.GetRepository();

            var result = await client.IsEnterprise();

            Assert.That(expected, Is.EqualTo(result));
            Assert.That(expected, Is.EqualTo(await client.IsEnterprise()));
        }

        [Test]
        public void ReturnsFalseWhenWeHaveNotRequestedRepository()
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var enterpriseProbe = Substitute.For<IEnterpriseProbe>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbe>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var result = client.IsEnterprise().Result;

            Assert.False(result);
        }
    }

    public class TheIsIsAuthenticatedMethod
    {
        [Test]
        public void ReturnsFalseWhenCredentialsNotSet()
        {
            var gitHubClient = Substitute.For<IGitHubClient>();
            gitHubClient.Connection.Credentials.Returns((Credentials)null);

            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                null,
                null);

            var result = client.IsAuthenticated();
            Assert.False(result);
        }

        [Test]
        public void ReturnsFalseWhenAuthenicationTypeIsAnonymous()
        {
            var connection = Substitute.For<IConnection>();
            connection.Credentials=Credentials.Anonymous;

            var gitHubClient = Substitute.For<IGitHubClient>();
            gitHubClient.Connection.Returns(connection);

            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                null,
                null);

            var result = client.IsAuthenticated();
            Assert.False(result);
        }

        [Test]
        public void ReturnsTrueWhenLoginIsSetToBasicAuth()
        {
            var connection = Substitute.For<IConnection>();
            connection.Credentials.Returns(new Credentials("username", "password"));

            var gitHubClient = Substitute.For<IGitHubClient>();
            gitHubClient.Connection.Returns(connection);

            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                null,
                null);

            var result = client.IsAuthenticated();
            Assert.True(result);
        }

        [Test]
        public void ReturnsTrueWhenLoginIsSetToOAuth()
        {
            var connection = Substitute.For<IConnection>();
            connection.Credentials.Returns(new Credentials("token"));

            var gitHubClient = Substitute.For<IGitHubClient>();
            gitHubClient.Connection.Returns(connection);

            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                null,
                null);

            var result = client.IsAuthenticated();
            Assert.True(result);
        }
    }

    private static Repository CreateRepository(int id, bool hasWiki)
    {
        return new Repository("", "", "", "", "", "", "",
            id, new User(), "", "", "", "", "", false, false, 0, 0, "",
            0, null, DateTimeOffset.Now, DateTimeOffset.Now, new RepositoryPermissions(), null, null, false,
            hasWiki, false, false, 0, 0, null, null, null);
    }
}
