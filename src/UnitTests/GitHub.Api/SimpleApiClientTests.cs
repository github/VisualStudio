using System;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Primitives;
using GitHub.Services;
using NSubstitute;
using Octokit;
using Xunit;

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
        [Fact]
        public async Task RetrievesRepositoryFromWeb()
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var repository = new Repository(42);
            gitHubClient.Repository.Get("github", "visualstudio").Returns(Task.FromResult(repository));
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var result = await client.GetRepository();

            Assert.Equal(42, result.Id);
        }

        [Fact]
        public async Task RetrievesCachedRepositoryForSubsequentCalls()
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var repository = new Repository(42);
            gitHubClient.Repository.Get("github", "visualstudio")
                .Returns(_ => Task.FromResult(repository), _ => { throw new Exception("Should only be called once."); });
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));
            await client.GetRepository();

            var result = await client.GetRepository();

            Assert.Equal(42, result.Id);
        }
    }

    public class TheHasWikiMethod
    {
        [Theory]
        [InlineData(WikiProbeResult.Ok, true)]
        [InlineData(WikiProbeResult.Failed, false)]
        [InlineData(WikiProbeResult.NotFound, false)]
        public async Task ReturnsTrueWhenWikiProbeReturnsOk(WikiProbeResult probeResult, bool expected)
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var repository = CreateRepository(42, true);
            gitHubClient.Repository.Get("github", "visualstudio").Returns(Task.FromResult(repository));
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            wikiProbe.ProbeAsync(repository)
                .Returns(_ => Task.FromResult(probeResult), _ => { throw new Exception("Only call it once"); });
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));
            await client.GetRepository();

            var result = client.HasWiki();

            Assert.Equal(expected, result);
            Assert.Equal(expected, client.HasWiki());
        }

        [Fact]
        public void ReturnsFalseWhenWeHaveNotRequestedRepository()
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var result = client.HasWiki();

            Assert.False(result);
        }
    }

    public class TheIsEnterpriseMethod
    {
        [Theory]
        [InlineData(EnterpriseProbeResult.Ok, true)]
        [InlineData(EnterpriseProbeResult.Failed, false)]
        [InlineData(EnterpriseProbeResult.NotFound, false)]
        public async Task ReturnsTrueWhenEnterpriseProbeReturnsOk(EnterpriseProbeResult probeResult, bool expected)
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var repository = CreateRepository(42, true);
            gitHubClient.Repository.Get("github", "visualstudio").Returns(Task.FromResult(repository));
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            enterpriseProbe.ProbeAsync(Args.Uri)
                .Returns(_ => Task.FromResult(probeResult), _ => { throw new Exception("Only call it once"); });
            var wikiProbe = Substitute.For<IWikiProbe>();
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));
            await client.GetRepository();

            var result = client.IsEnterprise();

            Assert.Equal(expected, result);
            Assert.Equal(expected, client.IsEnterprise());
        }

        [Fact]
        public void ReturnsFalseWhenWeHaveNotRequestedRepository()
        {
            var gitHubHost = HostAddress.GitHubDotComHostAddress;
            var gitHubClient = Substitute.For<IGitHubClient>();
            var enterpriseProbe = Substitute.For<IEnterpriseProbeTask>();
            var wikiProbe = Substitute.For<IWikiProbe>();
            var client = new SimpleApiClient(
                "https://github.com/github/visualstudio",
                gitHubClient,
                new Lazy<IEnterpriseProbeTask>(() => enterpriseProbe),
                new Lazy<IWikiProbe>(() => wikiProbe));

            var result = client.IsEnterprise();

            Assert.False(result);
        }
    }

    public class TheIsIsAuthenticatedMethod
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
            hasWiki, false);
    }
}
