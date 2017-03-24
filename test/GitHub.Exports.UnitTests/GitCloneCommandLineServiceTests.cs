using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;
using Rothko;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services.Tests
{
    public class GitCloneCommandLineServiceTests
    {
        [Test]
        public void NoGitCloneCommandLine_DontOpenRepository()
        {
            var vsAppCommandLine = CreateVsAppCommandLine(null);
            var vsServices = Substitute.For<IVSServices>();
            var operatingSystem = Substitute.For<IOperatingSystem>();

            var gitCloneCommandLine = new GitCloneCommandLineService(
                vsAppCommandLine, null, null, vsServices, operatingSystem);

            vsServices.DidNotReceiveWithAnyArgs().TryOpenRepository(null);
        }

        [Test]
        public void GitCloneCommandLine_UnknownRepo_DontOpenRepository()
        {
            var repoUrl = "https://github.com/unknown/unknown";
            var vsAppCommandLine = CreateVsAppCommandLine(repoUrl);
            var vsGitServices = Substitute.For<IVSGitServices>();
            var vsServices = Substitute.For<IVSServices>();
            var operatingSystem = Substitute.For<IOperatingSystem>();

            var gitCloneCommandLine = new GitCloneCommandLineService(
                vsAppCommandLine, vsGitServices, null, vsServices, operatingSystem);

            vsServices.DidNotReceiveWithAnyArgs().TryOpenRepository(null);
        }

        [Test]
        public void GitCloneCommandLine_KnownRepositoriesRepo_TryOpenRepository()
        {
            var repoUrl = "https://github.com/known/known";
            var localPath = @"x:\repo\path";
            var vsAppCommandLine = CreateVsAppCommandLine(repoUrl);
            var vsGitServices = Substitute.For<IVSGitServices>();
            var knownRepos = CreateKnownRepository(repoUrl, localPath);
            vsGitServices.GetKnownRepositories().Returns(knownRepos);
            var vsServices = Substitute.For<IVSServices>();
            var operatingSystem = Substitute.For<IOperatingSystem>();

            var gitCloneCommandLine = new GitCloneCommandLineService(
                vsAppCommandLine, vsGitServices, null, vsServices, operatingSystem);

            vsServices.Received(1).TryOpenRepository(localPath);
        }

        [TestCase("https://github.com/github/VisualStudio", @"x:\clone_path", @"x:\clone_path\github\VisualStudio", true)]
        [TestCase("https://github.com/github/VisualStudio", @"x:\clone_path", @"x:\clone_path\GITHUB\VISUALSTUDIO", true)]
        [TestCase("https://github.com/github/VisualStudio", @"x:\clone_path", null, false)]
        [TestCase("https://github.com/github/VisualStudio", null, null, false, Description = "LocalClonePath is null")]
        [TestCase("https://not-github.com/github/VisualStudio", @"x:\clone_path", @"x:\clone_path\github\VisualStudio", false)]
        [TestCase("https://github.com/github", @"x:\clone_path", @"x:\clone_path\github", false, Description = "No repo")]
        [TestCase("https://github.com", @"x:\clone_path", @"x:\clone_path", false, Description = "No owner")]
        [TestCase("NOT_A_URL", @"x:\clone_path", @"x:\clone_path", false, Description = "Not a URL")]
        public void GitCloneCommandLine_RepoExistsOnLocalClonePath_TryOpenRepository(
            string repoUrl, string clonePath, string localPath, bool tryOpenRepo)
        {
            var vsAppCommandLine = CreateVsAppCommandLine(repoUrl);
            var vsGitServices = Substitute.For<IVSGitServices>();
            vsGitServices.GetLocalClonePathFromGitProvider().Returns(clonePath);
            var vsServices = Substitute.For<IVSServices>();
            var operatingSystem = CreateOperatingSystemWithDirectory(localPath);

            var gitCloneCommandLine = new GitCloneCommandLineService(
                vsAppCommandLine, vsGitServices, null, vsServices, operatingSystem);

            if (tryOpenRepo)
            {
                vsServices.Received(1).TryOpenRepository(Arg.Is<string>(s => IgnoreCase(s, localPath)));
            }
            else
            {
                vsServices.DidNotReceiveWithAnyArgs().TryOpenRepository(null);
            }
        }

        static IOperatingSystem CreateOperatingSystemWithDirectory(string dirPath = null)
        {
            var operatingSystem = Substitute.For<IOperatingSystem>();
            if (dirPath != null)
            {
                operatingSystem.Directory.GetDirectory(Arg.Is<string>(s => IgnoreCase(s, dirPath))).Exists.Returns(true);
            }

            return operatingSystem;
        }

        static bool IgnoreCase(string s1, string s2) => string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);

        static IEnumerable<ILocalRepositoryModel> CreateKnownRepository(string repoUrl, string localPath)
        {
            var localRepositoryModel = Substitute.For<ILocalRepositoryModel>();
            localRepositoryModel.CloneUrl.Returns(new UriString(repoUrl));
            localRepositoryModel.LocalPath.Returns(localPath);
            var knownRepos = new[] { localRepositoryModel };
            return knownRepos;
        }

        static IVsAppCommandLine CreateVsAppCommandLine(string repoUrl)
        {
            var outPresent = (repoUrl == null) ? 0 : 1;
            var vsAppCommandLine = Substitute.For<IVsAppCommandLine>();
            vsAppCommandLine.GetOption("GitClone", out int present, out string optionValue).Returns(
                x => { x[1] = outPresent; x[2] = repoUrl; return 0; });
            return vsAppCommandLine;
        }
    }
}
