using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Services;
using Microsoft.TeamFoundation.Git.Controls.Extensibility;
using NSubstitute;
using Rothko;
using Xunit;

public class RepositoryCloneServiceTests
{
    public class TheCloneRepositoryMethod
    {
        [Fact]
        public async Task ClonesToRepositoryPath()
        {
            var gitClone = Substitute.For<IGitRepositoriesExt>();
            var operatingSystem = Substitute.For<IOperatingSystem>();
            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IGitRepositoriesExt)).Returns(gitClone);
            var cloneService = new RepositoryCloneService(
                new Lazy<IServiceProvider>(() => serviceProvider),
                operatingSystem);

            await cloneService.CloneRepository("https://github.com/foo/bar", "bar", @"c:\dev");

            operatingSystem.Directory.Received().CreateDirectory(@"c:\dev\bar");
            gitClone.Received().Clone("https://github.com/foo/bar", @"c:\dev\bar", CloneOptions.RecurseSubmodule);
        }
    }
}
