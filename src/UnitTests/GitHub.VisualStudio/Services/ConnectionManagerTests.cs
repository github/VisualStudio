using System;
using System.Text;
using GitHub.Models;
using GitHub.VisualStudio;
using NSubstitute;
using Rothko;
using Xunit;

public class ConnectionManagerTests
{
    public class TheConnectionsProperty
    {
        [Fact]
        public void IsLoadedFromCache()
        {
            const string cacheData = @"{""connections"":[{""HostUrl"":""https://github.com"", ""UserName"":""shana""},{""HostUrl"":""https://ghe.io"", ""UserName"":""haacked""}]}";
            var program = Substitute.For<IProgram>();
            program.ApplicationProvider.Returns("GHfVS");
            var operatingSystem = Substitute.For<IOperatingSystem>();
            operatingSystem.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
                .Returns(@"c:\fake");
            operatingSystem.File.Exists(@"c:\fake\GHfVS\ghfvs.connections").Returns(true);
            operatingSystem.File.ReadAllText(@"c:\fake\GHfVS\ghfvs.connections", Encoding.UTF8).Returns(cacheData);
            var manager = new ConnectionManager(program, operatingSystem);

            var connections = manager.Connections;

            Assert.Equal(2, connections.Count);
            Assert.Equal(new Uri("https://api.github.com"), connections[0].HostAddress.ApiUri);
            Assert.Equal(new Uri("https://ghe.io/api/v3/"), connections[1].HostAddress.ApiUri);
        }

        [Fact]
        public void IsEmptyWhenCacheCorrupt()
        {
            const string cacheData = @"|!This ain't no JSON what even is this?";
            var program = Substitute.For<IProgram>();
            program.ApplicationProvider.Returns("GHfVS");
            var operatingSystem = Substitute.For<IOperatingSystem>();
            operatingSystem.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
                .Returns(@"c:\fake");
            operatingSystem.File.Exists(@"c:\fake\GHfVS\ghfvs.connections").Returns(true);
            operatingSystem.File.ReadAllText(@"c:\fake\GHfVS\ghfvs.connections", Encoding.UTF8).Returns(cacheData);
            var manager = new ConnectionManager(program, operatingSystem);

            var connections = manager.Connections;

            Assert.Equal(0, connections.Count);
            operatingSystem.File.Received().Delete(@"c:\fake\GHfVS\ghfvs.connections");
        }
    }
}
