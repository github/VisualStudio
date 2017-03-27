using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;
using Rothko;
using GitHub.Models;
using GitHub.Primitives;
using Microsoft.VisualStudio;

namespace GitHub.Services.Tests
{
    public class GitCloneCommandLineServiceTests
    {
        public class TheFindGitCloneOptionMethod
        {
            [Test]
            public void OptionExists()
            {
                var gitCloneUrl = "https://github.com/foo/bar";
                var vsAppCommandLine = CreateVsAppCommandLine(gitCloneUrl);
                var target = new GitCloneCommandLineService(vsAppCommandLine, null, null, null, null);

                var cloneOption = target.FindGitCloneOption();

                Assert.That(cloneOption, Is.EqualTo(gitCloneUrl));
            }

            [Test]
            public void OptionDoesNotExist()
            {
                var vsAppCommandLine = CreateVsAppCommandLine(null);
                var target = new GitCloneCommandLineService(vsAppCommandLine, null, null, null, null);

                var cloneOption = target.FindGitCloneOption();

                Assert.That(cloneOption, Is.Null);
            }

            [Test]
            public void ErrorCode_ReturnsNull()
            {
                var gitCloneUrl = "https://github.com/foo/bar";
                var vsAppCommandLine = CreateVsAppCommandLine(gitCloneUrl, errorCode: VSConstants.E_FAIL);
                var target = new GitCloneCommandLineService(vsAppCommandLine, null, null, null, null);

                var cloneOption = target.FindGitCloneOption();

                Assert.That(cloneOption, Is.Null);
            }

            [Test]
            public void VsAppCommandLineIsNull_ReturnsNull()
            {
                IVsAppCommandLine vsAppCommandLine = null;
                var target = new GitCloneCommandLineService(
                    vsAppCommandLine, null, null, null, null);

                var cloneOption = target.FindGitCloneOption();

                Assert.That(cloneOption, Is.Null);
            }
        }

        public class TheTryOpenRepositoryMethod
        {
            [TestCase("https://github.com/github/VisualStudio", @"x:\clone_path", @"x:\clone_path\github\VisualStudio", true)]
            [TestCase("https://github.com/github/VisualStudio", @"x:\clone_path", @"x:\clone_path\GITHUB\VISUALSTUDIO", true)]
            [TestCase("https://github.com/github/VisualStudio", @"x:\clone_path", null, false)]
            [TestCase("https://github.com/github/VisualStudio", null, null, false, Description = "LocalClonePath is null")]
            [TestCase("https://not-github.com/github/VisualStudio", @"x:\clone_path", @"x:\clone_path\github\VisualStudio", false)]
            [TestCase("https://github.com/github", @"x:\clone_path", @"x:\clone_path\github", false, Description = "No repo")]
            [TestCase("https://github.com", @"x:\clone_path", @"x:\clone_path", false, Description = "No owner")]
            [TestCase("NOT_A_URL", @"x:\clone_path", @"x:\clone_path", false, Description = "Not a URL")]
            public void RepoExistsOnLocalClonePath_TryOpenRepository(
                string repoUrl, string clonePath, string localPath, bool tryOpenRepo)
            {
                var vsGitServices = Substitute.For<IVSGitServices>();
                vsGitServices.GetLocalClonePathFromGitProvider().Returns(clonePath);
                var vsServices = CreateVSServices();
                var operatingSystem = CreateOperatingSystemWithDirectory(localPath);
                var target = new GitCloneCommandLineService(
                    null, vsGitServices, null, vsServices, operatingSystem);

                var opened = target.TryOpenRepository(repoUrl);

                Assert.That(opened, Is.EqualTo(tryOpenRepo));
                if (tryOpenRepo)
                {
                    vsServices.Received(1).TryOpenRepository(Arg.Is<string>(s => IgnoreCase(s, localPath)));
                }
                else
                {
                    vsServices.DidNotReceiveWithAnyArgs().TryOpenRepository(null);
                }
            }

            [Test]
            public void KnownRepositoriesRepo_TryOpenRepository()
            {
                var repoUrl = "https://github.com/known/known";
                var localPath = @"x:\repo\path";
                var vsGitServices = Substitute.For<IVSGitServices>();
                var knownRepos = CreateKnownRepository(repoUrl, localPath);
                vsGitServices.GetKnownRepositories().Returns(knownRepos);
                var vsServices = CreateVSServices();
                var operatingSystem = Substitute.For<IOperatingSystem>();
                var target = new GitCloneCommandLineService(
                    null, vsGitServices, null, vsServices, operatingSystem);

                var opened = target.TryOpenRepository(repoUrl);

                Assert.That(opened, Is.True);
                vsServices.Received(1).TryOpenRepository(localPath);
            }

            [Test]
            public void UnknownRepo_DontOpenRepository()
            {
                var repoUrl = "https://github.com/unknown/unknown";
                var vsGitServices = Substitute.For<IVSGitServices>();
                var vsServices = CreateVSServices();
                var operatingSystem = Substitute.For<IOperatingSystem>();
                var target = new GitCloneCommandLineService(
                    null, vsGitServices, null, vsServices, operatingSystem);

                var opened = target.TryOpenRepository(repoUrl);

                Assert.That(opened, Is.False);
                vsServices.DidNotReceiveWithAnyArgs().TryOpenRepository(null);
            }


            [Test]
            public void RepoExistsOnLocalClonePathAndKnownRepositories_PrioritizeLocalClonePath()
            {
                var owner = "__OWNER__";
                var repo = "__REPO__";
                var repoUrl = $"https://github.com/{owner}/{repo}";
                var localPath = @"x:\repo\path";
                var clonePath = @"x:\clone_path";
                var expectedPath = $"{clonePath}\\{owner}\\{repo}";
                var vsGitServices = Substitute.For<IVSGitServices>();
                var knownRepos = CreateKnownRepository(repoUrl, localPath);
                vsGitServices.GetKnownRepositories().Returns(knownRepos);
                vsGitServices.GetLocalClonePathFromGitProvider().Returns(clonePath);
                var vsServices = CreateVSServices();
                var operatingSystem = CreateOperatingSystemWithDirectory(expectedPath);
                var target = new GitCloneCommandLineService(null, vsGitServices, null, vsServices, operatingSystem);

                var opened = target.TryOpenRepository(repoUrl);

                Assert.That(opened, Is.True);
                vsServices.Received(1).TryOpenRepository(expectedPath);
                vsServices.Received(1).TryOpenRepository(Arg.Any<string>());
            }
        }

        static IVSServices CreateVSServices()
        {
            var vsServices = Substitute.For<IVSServices>();
            vsServices.TryOpenRepository(Arg.Any<string>()).Returns(true);
            return vsServices;
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

        static IVsAppCommandLine CreateVsAppCommandLine(string repoUrl, int errorCode = VSConstants.S_OK)
        {
            var outPresent = (repoUrl == null) ? 0 : 1;
            var vsAppCommandLine = Substitute.For<IVsAppCommandLine>();
            vsAppCommandLine.GetOption("GitClone", out int present, out string optionValue).Returns(
                x => { x[1] = outPresent; x[2] = repoUrl; return errorCode; });
            return vsAppCommandLine;
        }
    }
}
