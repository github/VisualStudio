using System;
using System.IO;
using System.Runtime.InteropServices;
using GitHub.Services;
using NSubstitute;
using Xunit;
using DTE = EnvDTE.DTE;
using Rothko;

public class TeamExplorerServicesTests
{
    public class TheTryOpenRepositoryMethod : TestBaseClass
    {
        [Fact]
        public void NoExceptions_ReturnsTrue()
        {
            var repoDir = @"x:\repo";
            var target = CreateVSGitServices(repoDir);

            var success = target.TryOpenRepository(repoDir);

            Assert.True(success);
        }

        [Fact]
        public void SolutionCreateThrows_ReturnsFalse()
        {
            var repoDir = @"x:\repo";
            var dte = Substitute.For<DTE>();
            dte.Solution.When(s => s.Create(Arg.Any<string>(), Arg.Any<string>())).Do(
                ci => { throw new COMException(); });
            var target = CreateVSGitServices(repoDir, dte: dte);

            var success = target.TryOpenRepository("");

            Assert.False(success);
        }

        [Fact]
        public void RepoDirExistsFalse_ReturnFalse()
        {
            var repoDir = @"x:\repo";
            var os = Substitute.For<IOperatingSystem>();
            //var directoryInfo = Substitute.For<IDirectoryInfo>();
            //directoryInfo.Exists.Returns(false);
            //os.Directory.GetDirectory(repoDir).Returns(directoryInfo);
            var target = CreateVSGitServices(null, os: os);

            var success = target.TryOpenRepository(repoDir);

            Assert.False(success);
        }

        [Fact]
        public void DeleteThrowsIOException_ReturnTrue()
        {
            var repoDir = @"x:\repo";
            var tempDir = Path.Combine(repoDir, ".vs", VSGitServices.TempSolutionName);
            var os = Substitute.For<IOperatingSystem>();
            var directoryInfo = Substitute.For<IDirectoryInfo>();
            directoryInfo.Exists.Returns(true);
            os.Directory.GetDirectory(tempDir).Returns(directoryInfo);
            directoryInfo.When(di => di.Delete(true)).Do(
                ci => { throw new IOException(); });
            var target = CreateVSGitServices(repoDir, os: os);

            var success = target.TryOpenRepository(repoDir);

            Assert.True(success);
        }

        [Fact]
        public void SolutionCreate_DeleteVsSolutionSubdir()
        {
            var repoDir = @"x:\repo";
            var tempDir = Path.Combine(repoDir, ".vs", VSGitServices.TempSolutionName);
            var os = Substitute.For<IOperatingSystem>();
            var directoryInfo = Substitute.For<IDirectoryInfo>();
            directoryInfo.Exists.Returns(true);
            os.Directory.GetDirectory(tempDir).Returns(directoryInfo);
            var target = CreateVSGitServices(repoDir, os: os);

            var success = target.TryOpenRepository(repoDir);

            directoryInfo.Received().Delete(true);
        }

        VSGitServices CreateVSGitServices(string repoDir, IOperatingSystem os = null, DTE dte = null)
        {
            os = os ?? Substitute.For<IOperatingSystem>();
            dte = dte ?? Substitute.For<DTE>();

            if (repoDir != null)
            {
                var directoryInfo = Substitute.For<IDirectoryInfo>();
                directoryInfo.Exists.Returns(true);
                os.Directory.GetDirectory(repoDir).Returns(directoryInfo);
            }

            var provider = Substitute.For<IGitHubServiceProvider>();
            provider.TryGetService<DTE>().Returns(dte);
            provider.TryGetService<IOperatingSystem>().Returns(os);
            return new VSGitServices(provider);
        }
    }
}
