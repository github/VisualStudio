using System;
using System.IO;
using System.Runtime.InteropServices;
using GitHub.Services;
using NSubstitute;
using Xunit;
using DTE = EnvDTE.DTE;
using Rothko;

public class VSServicesTests
{
    public class TryOpenRepository : TestBaseClass
    {
        [Fact]
        public void NoExceptions_ReturnsTrue()
        {
            var target = CreateVSServices();

            var success = target.TryOpenRepository("");

            Assert.True(success);
        }

        [Fact]
        public void SolutionCreateThrows_ReturnsFalse()
        {
            var dte = Substitute.For<DTE>();
            dte.Solution.When(s => s.Create(Arg.Any<string>(), Arg.Any<string>())).Do(
                ci => { throw new COMException(); });
            var target = CreateVSServices(dte: dte);

            var success = target.TryOpenRepository("");

            Assert.False(success);
        }

        [Fact]
        public void SolutionCreate_DeleteVsSolutionSubdir()
        {
            var repoDir = @"x:\repo";
            var tempDir = Path.Combine(repoDir, ".vs", VSServices.TempSolutionName);
            var os = Substitute.For<IOperatingSystem>();
            var directoryInfo = Substitute.For<IDirectoryInfo>();
            directoryInfo.Exists.Returns(true);
            os.Directory.GetDirectory(tempDir).Returns(directoryInfo);
            var target = CreateVSServices(os: os);

            var success = target.TryOpenRepository(repoDir);

            directoryInfo.Received().Delete(true);
        }

        VSServices CreateVSServices(IOperatingSystem os = null, DTE dte = null)
        {
            os = os ?? Substitute.For<IOperatingSystem>();
            dte = dte ?? Substitute.For<DTE>();
            var provider = Substitute.For<IGitHubServiceProvider>();
            provider.GetService(typeof(DTE)).Returns(dte);
            provider.GetService(typeof(IOperatingSystem)).Returns(os);
            return new VSServices(provider);
        }
    }

    public class TheCloneMethod : TestBaseClass
    {
        /*
        [Theory]
        [InlineData(true, CloneOptions.RecurseSubmodule)]
        [InlineData(false, CloneOptions.None)]
        public void CallsCloneOnVsProvidedCloneService(bool recurseSubmodules, CloneOptions expectedCloneOptions)
        {
            var provider = Substitute.For<IUIProvider>();
            var gitRepositoriesExt = Substitute.For<IGitRepositoriesExt>();
            provider.GetService(typeof(IGitRepositoriesExt)).Returns(gitRepositoriesExt);
            provider.TryGetService(typeof(IGitRepositoriesExt)).Returns(gitRepositoriesExt);
            var vsServices = new VSServices(provider);

            vsServices.Clone("https://github.com/github/visualstudio", @"c:\fake\ghfvs", recurseSubmodules);

            gitRepositoriesExt.Received()
                .Clone("https://github.com/github/visualstudio", @"c:\fake\ghfvs", expectedCloneOptions);
        }
        */
    }
}
