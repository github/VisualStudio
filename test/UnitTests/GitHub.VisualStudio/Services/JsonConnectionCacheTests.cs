using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.VisualStudio;
using NSubstitute;
using Rothko;
using NUnit.Framework;

public class JsonConnectionCacheTests
{
    public class TheLoadMethod : TestBaseClass
    {
        [Test]
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
            Assert.That(2, Is.EqualTo(connections.Count));
            Assert.That(new Uri("https://api.github.com"), Is.EqualTo(connections[0].HostAddress.ApiUri));
            Assert.That(new Uri("https://github.enterprise/api/v3/"), Is.EqualTo(connections[1].HostAddress.ApiUri));
        }

        [TestCase("|!This ain't no JSON what even is this?")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("{}")]
        [TestCase(@"{""connections"":null}")]
        [TestCase(@"{""connections"":{}}")]
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

            Assert.That(connections, Is.Empty);
            operatingSystem.File.Received().Delete(@"c:\fake\GHfVS\ghfvs.connections");
        }

        [Test]
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
