using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Services;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using NSubstitute;
using Rothko;
using Xunit;
using UnitTests;

public class RepositoryCloneServiceTests
{
    public class TheCloneRepositoryMethod : TestBaseClass
    {
        [Fact]
        public async Task ClonesToRepositoryPath()
        {
            var serviceProvider = Substitutes.ServiceProvider;
            var operatingSystem = serviceProvider.GetOperatingSystem();
            var vsservices = serviceProvider.GetVSServices();
            var cloneService = serviceProvider.GetRepositoryCloneService();

            await cloneService.CloneRepository("https://github.com/foo/bar", "bar", @"c:\dev");

            operatingSystem.Directory.Received().CreateDirectory(@"c:\dev\bar");
            vsservices.Received().Clone("https://github.com/foo/bar", @"c:\dev\bar", true);
        }
    }
}
