using System;
using System.IO;
using GitHub.Services;
using NUnit.Framework;
using NSubstitute;
using EnvDTE;
using GitHub.Models;

namespace GitHub.App.UnitTests.Services
{
    public class TeamExplorerContextTests
    {
        public class TheActiveRepositoryProperty
        {
            [Test]
            public void NoActiveRepository()
            {
                var gitExt = CreateGitExt();
                var target = CreateTeamExplorerContext(gitExt);

                var repo = target.ActiveRepository;

                Assert.That(repo, Is.Null);
            }

            [Test]
            public void SetActiveRepository_CheckWasSet()
            {
                var gitExt = CreateGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = CreateRepositoryModel(repositoryPath);
                SetActiveRepository(gitExt, repoInfo);
                var target = CreateTeamExplorerContext(gitExt);

                var repo = target.ActiveRepository;

                Assert.That(repo, Is.EqualTo(repoInfo));
            }
        }

        public class ThePropertyChangedEvent
        {
            [Test]
            public void SetActiveRepository_CheckEventWasRaised()
            {
                var gitExt = CreateGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = CreateRepositoryModel(repositoryPath);
                var target = CreateTeamExplorerContext(gitExt);
                var eventWasRaised = false;
                target.PropertyChanged += (s, e) => eventWasRaised = e.PropertyName == nameof(target.ActiveRepository);

                SetActiveRepository(gitExt, repoInfo);

                Assert.That(eventWasRaised, Is.True);
            }

            [Test]
            public void SetTwicePropertyChangedFiresOnce()
            {
                var gitExt = CreateGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = CreateRepositoryModel(repositoryPath);
                var target = CreateTeamExplorerContext(gitExt);
                var eventWasRaisedCount = 0;
                target.PropertyChanged += (s, e) => eventWasRaisedCount++;

                SetActiveRepository(gitExt, repoInfo);
                SetActiveRepository(gitExt, repoInfo);

                Assert.That(1, Is.EqualTo(1));
            }

            [Test]
            public void ChangeActiveRepository_NoSolutionChange()
            {
                var gitExt = CreateGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = CreateRepositoryModel(repositoryPath);
                var repositoryPath2 = Path.GetTempPath();
                var repoInfo2 = CreateRepositoryModel(repositoryPath2);
                var target = CreateTeamExplorerContext(gitExt);
                SetActiveRepository(gitExt, repoInfo);
                var eventWasRaised = false;
                target.PropertyChanged += (s, e) => eventWasRaised = e.PropertyName == nameof(target.ActiveRepository);

                SetActiveRepository(gitExt, repoInfo2);

                Assert.That(eventWasRaised, Is.True);
            }

            [Test]
            public void ClearActiveRepository_NoEventWhenNoSolutionChange()
            {
                var gitExt = CreateGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = CreateRepositoryModel(repositoryPath);
                var target = CreateTeamExplorerContext(gitExt);
                SetActiveRepository(gitExt, repoInfo);
                var eventWasRaised = false;
                target.PropertyChanged += (s, e) => eventWasRaised = e.PropertyName == nameof(target.ActiveRepository);

                SetActiveRepository(gitExt, null);

                Assert.That(eventWasRaised, Is.False);
                Assert.That(target.ActiveRepository, Is.EqualTo(repoInfo));
            }

            [Test]
            public void ClearActiveRepository_FireWhenSolutionChanged()
            {
                var gitExt = CreateGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = CreateRepositoryModel(repositoryPath);
                var dte = Substitute.For<DTE>();
                var target = CreateTeamExplorerContext(gitExt, dte);
                dte.Solution.FullName.Returns("Solution1");
                SetActiveRepository(gitExt, repoInfo);
                var eventWasRaised = false;
                target.PropertyChanged += (s, e) => eventWasRaised = e.PropertyName == nameof(target.ActiveRepository);

                dte.Solution.FullName.Returns("Solution2");
                SetActiveRepository(gitExt, null);

                Assert.That(eventWasRaised, Is.True);
                Assert.That(target.ActiveRepository, Is.Null);
            }

            [Test]
            public void NoActiveRepositoryChange_SolutionChanges()
            {
                var gitExt = CreateGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = CreateRepositoryModel(repositoryPath);
                var dte = Substitute.For<DTE>();
                var target = CreateTeamExplorerContext(gitExt, dte);
                dte.Solution.FullName.Returns("");
                SetActiveRepository(gitExt, repoInfo);
                var eventWasRaised = false;
                target.PropertyChanged += (s, e) => eventWasRaised = e.PropertyName == nameof(target.ActiveRepository);

                dte.Solution.FullName.Returns("Solution");
                SetActiveRepository(gitExt, repoInfo);

                Assert.That(eventWasRaised, Is.False);
            }
        }

        public class TheStatusChangedEvent
        {
            [TestCase(false, "name1", "sha1", "name1", "sha1", false)]
            [TestCase(false, "name1", "sha1", "name2", "sha1", true)]
            [TestCase(false, "name1", "sha1", "name1", "sha2", true)]
            [TestCase(false, "name1", "sha1", "name2", "sha2", true)]
            [TestCase(true, "name1", "sha1", "name1", "sha1", false)]
            [TestCase(true, "name1", "sha1", "name2", "sha2", false)]
            public void SameActiveRepository_ExpectWasRaised(bool changePath, string name1, string sha1, string name2, string sha2, bool expectWasRaised)
            {
                var gitExt = CreateGitExt();
                var repositoryPaths = new[] { Directory.GetCurrentDirectory(), Path.GetTempPath() };
                var path1 = Directory.GetCurrentDirectory();
                var path2 = changePath ? Path.GetTempPath() : path1;
                var repoInfo1 = CreateRepositoryModel(path1, name1, sha1);
                var repoInfo2 = CreateRepositoryModel(path2, name2, sha2);

                var target = CreateTeamExplorerContext(gitExt);
                var eventWasRaised = false;
                target.StatusChanged += (s, e) => eventWasRaised = true;

                SetActiveRepository(gitExt, repoInfo1);
                SetActiveRepository(gitExt, repoInfo2);

                Assert.That(eventWasRaised, Is.EqualTo(expectWasRaised));
            }

            [TestCase("trackedSha", "trackedSha", false)]
            [TestCase("trackedSha1", "trackedSha2", true)]
            public void TrackedShaChanges_CheckWasRaised(string trackedSha1, string trackedSha2, bool expectWasRaised)
            {
                var gitExt = CreateGitExt();
                var repositoryPaths = new[] { Directory.GetCurrentDirectory(), Path.GetTempPath() };
                var repoPath = Directory.GetCurrentDirectory();
                var repoInfo1 = CreateRepositoryModel(repoPath, "name", "sha", trackedSha1);
                var repoInfo2 = CreateRepositoryModel(repoPath, "name", "sha", trackedSha2);
                var target = CreateTeamExplorerContext(gitExt);
                SetActiveRepository(gitExt, repoInfo1);
                var eventWasRaised = false;
                target.StatusChanged += (s, e) => eventWasRaised = true;

                SetActiveRepository(gitExt, repoInfo2);

                Assert.That(eventWasRaised, Is.EqualTo(expectWasRaised));
            }

            [Test]
            public void SolutionUnloadedAndReloaded_DontFireStatusChanged()
            {
                var gitExt = CreateGitExt();
                var path = Directory.GetCurrentDirectory();
                var repoInfo1 = CreateRepositoryModel(path, "name", "sha");
                var repoInfo2 = CreateRepositoryModel(null);
                var target = CreateTeamExplorerContext(gitExt);
                SetActiveRepository(gitExt, repoInfo1);
                SetActiveRepository(gitExt, repoInfo2);

                var eventWasRaised = false;
                target.StatusChanged += (s, e) => eventWasRaised = true;
                SetActiveRepository(gitExt, repoInfo1);

                Assert.That(eventWasRaised, Is.False);
            }
        }

        static TeamExplorerContext CreateTeamExplorerContext(IVSGitExt gitExt, DTE dte = null)
        {
            dte = dte ?? Substitute.For<DTE>();
            var sp = Substitute.For<IGitHubServiceProvider>();
            sp.TryGetService<DTE>().Returns(dte);
            return new TeamExplorerContext(gitExt, sp);
        }

        static ILocalRepositoryModel CreateRepositoryModel(string path, string branchName = null, string headSha = null, string trackedSha = null)
        {
            var repo = Substitute.For<ILocalRepositoryModel>();
            repo.LocalPath.Returns(path);
            var currentBranch = Substitute.For<IBranch>();
            currentBranch.Name.Returns(branchName);
            currentBranch.Sha.Returns(headSha);
            currentBranch.TrackedSha.Returns(trackedSha);
            repo.CurrentBranch.Returns(currentBranch);
            return repo;
        }

        static IVSGitExt CreateGitExt()
        {
            return Substitute.For<IVSGitExt>();
        }

        static void SetActiveRepository(IVSGitExt gitExt, ILocalRepositoryModel repo)
        {
            var repos = repo != null ? new[] { repo } : new ILocalRepositoryModel[0];
            gitExt.ActiveRepositories.Returns(repos);
            gitExt.ActiveRepositoriesChanged += Raise.Event<Action>();
        }
    }
}
