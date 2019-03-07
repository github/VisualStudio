using System;
using System.IO;
using System.Runtime.InteropServices;
using GitHub.Services;
using NSubstitute;
using NUnit.Framework;
using DTE = EnvDTE.DTE;
using Rothko;
using Serilog;

public class VSServicesTests
{
    public class TheTryOpenRepositoryMethod : TestBaseClass
    {
        [Test]
        public void NoExceptions_ReturnsTrue()
        {
            var repoDir = @"x:\repo";
            var target = CreateVSServices(repoDir);

            var success = target.TryOpenRepository(repoDir);

            Assert.True(success);
        }

        [Test]
        public void SolutionCreateThrows_ReturnsFalse()
        {
            var repoDir = @"x:\repo";
            var dte = Substitute.For<DTE>();
            var log = Substitute.For<ILogger>();
            var ex = new COMException();
            dte.Solution.When(s => s.Create(Arg.Any<string>(), Arg.Any<string>())).Do(ci => { throw ex; });
            var target = CreateVSServices(repoDir, dte: dte, log: log);

            var success = target.TryOpenRepository(repoDir);

            Assert.False(success);
            log.Received(1).Error(ex, "Error opening repository");
        }

        [Test]
        public void RepoDirExistsFalse_ReturnFalse()
        {
            var repoDir = @"x:\repo";
            var target = CreateVSServices(repoDir, repoDirExists: false);

            var success = target.TryOpenRepository(repoDir);

            Assert.False(success);
        }

        [Test]
        public void DeleteThrowsIOException_ReturnTrue()
        {
            var repoDir = @"x:\repo";
            var tempDir = Path.Combine(repoDir, ".vs", VSServices.TempSolutionName);
            var os = Substitute.For<IOperatingSystem>();
            var directoryInfo = Substitute.For<IDirectoryInfo>();
            directoryInfo.Exists.Returns(true);
            os.Directory.GetDirectory(tempDir).Returns(directoryInfo);
            directoryInfo.When(di => di.Delete(true)).Do(
                ci => { throw new IOException(); });
            var target = CreateVSServices(repoDir, os: os);

            var success = target.TryOpenRepository(repoDir);

            Assert.True(success);
        }

        [Test]
        public void SolutionCreate_DeleteVsSolutionSubdir()
        {
            var repoDir = @"x:\repo";
            var tempDir = Path.Combine(repoDir, ".vs", VSServices.TempSolutionName);
            var os = Substitute.For<IOperatingSystem>();
            var directoryInfo = Substitute.For<IDirectoryInfo>();
            directoryInfo.Exists.Returns(true);
            os.Directory.GetDirectory(tempDir).Returns(directoryInfo);
            var target = CreateVSServices(repoDir, os: os);

            var success = target.TryOpenRepository(repoDir);

            directoryInfo.Received().Delete(true);
        }

        static VSServices CreateVSServices(string repoDir, IOperatingSystem os = null, DTE dte = null, bool repoDirExists = true, ILogger log = null)
        {
            os = os ?? Substitute.For<IOperatingSystem>();
            dte = dte ?? Substitute.For<DTE>();
            log = log ?? Substitute.For<ILogger>();

            if (repoDir != null)
            {
                var gitDir = Path.Combine(repoDir, ".git");
                var directoryInfo = Substitute.For<IDirectoryInfo>();
                directoryInfo.Exists.Returns(repoDirExists);
                os.Directory.GetDirectory(gitDir).Returns(directoryInfo);
            }

            var provider = Substitute.For<IGitHubServiceProvider>();
            provider.TryGetService<DTE>().Returns(dte);
            provider.TryGetService<IOperatingSystem>().Returns(os);
            return new VSServices(provider, log);
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
