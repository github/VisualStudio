using System;
using System.Text;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using GitHub.VisualStudio;
using NSubstitute;
using Rothko;
using Xunit;
using UnitTests;
using System.Collections.Generic;
using System.Linq;
using GitHub.Factories;
using GitHub.Api;

public class ConnectionManagerTests
{
    public class TheConnectionsProperty : TestBaseClass
    {
        [Fact]
        public void IsSavedToDiskWhenConnectionAdded()
        {
            var program = Substitute.For<IProgram>();
            program.ApplicationName.Returns("GHfVS");
            var operatingSystem = Substitute.For<IOperatingSystem>();
            operatingSystem.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
                .Returns(@"c:\fake");
            operatingSystem.File.Exists(@"c:\fake\GHfVS\ghfvs.connections").Returns(true);
            operatingSystem.File.ReadAllText(@"c:\fake\GHfVS\ghfvs.connections", Encoding.UTF8).Returns("");
            var cache = Substitute.For<IConnectionCache>();
            var loginManager = Substitute.For<ILoginManager>();
            var apiClientFactory = Substitute.For<IApiClientFactory>();
            var manager = new ConnectionManager(program, Substitutes.IVSGitServices, cache, loginManager, apiClientFactory);

            manager.Connections.Add(new Connection(manager, HostAddress.GitHubDotComHostAddress, "coolio"));

            Assert.Equal(1, manager.Connections.Count);
            cache.Received(1).Save(Arg.Is<IEnumerable<ConnectionDetails>>(x =>
                x.SequenceEqual(new[] { new ConnectionDetails(HostAddress.GitHubDotComHostAddress, "coolio") })));
        }
    }
}
