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

public class ConnectionManagerTests
{
    public class TheConnectionsProperty : TestBaseClass
    {
        [Fact]
        public void IsLoadedFromCache()
        {
            const string cacheData = @"{""connections"":[{""HostUrl"":""https://github.com"", ""UserName"":""shana""},{""HostUrl"":""https://ghe.io"", ""UserName"":""haacked""}]}";
            var program = Substitute.For<IProgram>();
            program.ApplicationName.Returns("GHfVS");
            var operatingSystem = Substitute.For<IOperatingSystem>();
            operatingSystem.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
                .Returns(@"c:\fake");
            operatingSystem.File.Exists(@"c:\fake\GHfVS\ghfvs.connections").Returns(true);
            operatingSystem.File.ReadAllText(@"c:\fake\GHfVS\ghfvs.connections", Encoding.UTF8).Returns(cacheData);
            var manager = new ConnectionManager(program, operatingSystem, Substitutes.IVSGitServices);

            var connections = manager.Connections;

            Assert.Equal(2, connections.Count);
            Assert.Equal(new Uri("https://api.github.com"), connections[0].HostAddress.ApiUri);
            Assert.Equal(new Uri("https://ghe.io/api/v3/"), connections[1].HostAddress.ApiUri);
        }

        [Theory]
        [InlineData("|!This ain't no JSON what even is this?")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("{}")]
        [InlineData(@"{""connections"":null}")]
        [InlineData(@"{""connections"":{}}")]
        public void IsEmptyWhenCacheCorrupt(string cacheJson)
        {
            var program = Substitute.For<IProgram>();
            program.ApplicationName.Returns("GHfVS");
            var operatingSystem = Substitute.For<IOperatingSystem>();
            operatingSystem.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
                .Returns(@"c:\fake");
            operatingSystem.File.Exists(@"c:\fake\GHfVS\ghfvs.connections").Returns(true);
            operatingSystem.File.ReadAllText(@"c:\fake\GHfVS\ghfvs.connections", Encoding.UTF8).Returns(cacheJson);
            var manager = new ConnectionManager(program, operatingSystem, Substitutes.IVSGitServices);

            var connections = manager.Connections;

            Assert.Equal(0, connections.Count);
            operatingSystem.File.Received().Delete(@"c:\fake\GHfVS\ghfvs.connections");
        }

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
            var manager = new ConnectionManager(program, operatingSystem, Substitutes.IVSGitServices);

            manager.Connections.Add(new Connection(manager, HostAddress.GitHubDotComHostAddress, "coolio"));

            Assert.Equal(1, manager.Connections.Count);
            operatingSystem.File.Received().WriteAllText(@"c:\fake\GHfVS\ghfvs.connections",
                @"{""connections"":[{""HostUrl"":""https://github.com/"",""UserName"":""coolio""}]}");
        }
    }
}
