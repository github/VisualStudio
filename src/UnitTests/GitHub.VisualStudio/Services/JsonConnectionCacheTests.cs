using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.VisualStudio;
using NSubstitute;
using Rothko;
using Xunit;

public class JsonConnectionCacheTests
{
    public class TheLoadMethod : TestBaseClass
    {
        [Fact]
        public async Task IsLoadedFromCache()
        {
            const string cacheData = @"{""connections"":[{""HostUrl"":""https://github.com"", ""UserName"":""shana""},{""HostUrl"":""https://github.enterprise"", ""UserName"":""haacked""}]}";
            var program = Substitute.For<IProgram>();
            program.ApplicationName.Returns("GHfVS");
            var operatingSystem = Substitute.For<IOperatingSystem>();
            operatingSystem.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
                .Returns(@"c:\fake");
            operatingSystem.File.Exists(@"c:\fake\GHfVS\ghfvs.connections").Returns(true);
            operatingSystem.File.ReadAllText(@"c:\fake\GHfVS\ghfvs.connections", Encoding.UTF8).Returns(cacheData);
            var cache = new JsonConnectionCache(program, operatingSystem);

            var connections = (await cache.Load()).ToList();
            Assert.Equal(2, connections.Count);
            Assert.Equal(new Uri("https://api.github.com"), connections[0].HostAddress.ApiUri);
            Assert.Equal(new Uri("https://github.enterprise/api/v3/"), connections[1].HostAddress.ApiUri);
        }

        [Theory]
        [InlineData("|!This ain't no JSON what even is this?")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("{}")]
        [InlineData(@"{""connections"":null}")]
        [InlineData(@"{""connections"":{}}")]
        public async Task IsEmptyWhenCacheCorrupt(string cacheJson)
        {
            var program = Substitute.For<IProgram>();
            program.ApplicationName.Returns("GHfVS");
            var operatingSystem = Substitute.For<IOperatingSystem>();
            operatingSystem.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
                .Returns(@"c:\fake");
            operatingSystem.File.Exists(@"c:\fake\GHfVS\ghfvs.connections").Returns(true);
            operatingSystem.File.ReadAllText(@"c:\fake\GHfVS\ghfvs.connections", Encoding.UTF8).Returns(cacheJson);
            var cache = new JsonConnectionCache(program, operatingSystem);

            var connections = (await cache.Load()).ToList();

            Assert.Equal(0, connections.Count);
            operatingSystem.File.Received().Delete(@"c:\fake\GHfVS\ghfvs.connections");
        }

        [Fact]
        public async Task IsSavedToDisk()
        {
            var program = Substitute.For<IProgram>();
            program.ApplicationName.Returns("GHfVS");
            var operatingSystem = Substitute.For<IOperatingSystem>();
            operatingSystem.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData)
                .Returns(@"c:\fake");
            operatingSystem.File.Exists(@"c:\fake\GHfVS\ghfvs.connections").Returns(true);
            operatingSystem.File.ReadAllText(@"c:\fake\GHfVS\ghfvs.connections", Encoding.UTF8).Returns("");
            var cache = new JsonConnectionCache(program, operatingSystem);
            var connections = (await cache.Load()).ToList();

            connections.Add(new ConnectionDetails(HostAddress.GitHubDotComHostAddress, "coolio"));
            await cache.Save(connections);

            operatingSystem.File.Received().WriteAllText(@"c:\fake\GHfVS\ghfvs.connections",
                @"{""connections"":[{""HostUrl"":""https://github.com/"",""UserName"":""coolio""}]}");
        }
    }
}
