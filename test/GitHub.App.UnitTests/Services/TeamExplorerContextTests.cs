using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using GitHub.Services;
using NUnit.Framework;
using NSubstitute;
using EnvDTE;
using Serilog;

namespace GitHub.App.UnitTests.Services
{
    public class TeamExplorerContextTests
    {
        public class TheActiveRepositoryProperty
        {
            [SetUp]
            public void SetUp()
            {
                Splat.ModeDetector.Current.SetInUnitTestRunner(true);
            }

            [Test]
            public void NoActiveRepository()
            {
                var gitExt = new FakeGitExt();
                var target = CreateTeamExplorerContext(gitExt);

                var repo = target.ActiveRepository;

                Assert.That(repo, Is.Null);
            }

            [Test]
            public void SetActiveRepository()
            {
                var gitExt = new FakeGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = new GitRepositoryInfo(repositoryPath, null);
                gitExt.SetActiveRepository(repoInfo);
                var target = CreateTeamExplorerContext(gitExt);

                var repo = target.ActiveRepository;

                Assert.That(repo.LocalPath, Is.EqualTo(repositoryPath));
            }
        }

        public class ThePropertyChangedEvent
        {
            [Test]
            public void SetActiveRepository()
            {
                var gitExt = new FakeGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = new GitRepositoryInfo(repositoryPath, null);
                var target = CreateTeamExplorerContext(gitExt);
                var eventWasRaised = false;
                target.PropertyChanged += (s, e) => eventWasRaised = e.PropertyName == nameof(target.ActiveRepository);

                gitExt.SetActiveRepository(repoInfo);

                Assert.That(eventWasRaised, Is.True);
            }

            [Test]
            public void SetTwicePropertyChangedFiresOnce()
            {
                var gitExt = new FakeGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = new GitRepositoryInfo(repositoryPath, null);
                var target = CreateTeamExplorerContext(gitExt);
                var eventWasRaisedCount = 0;
                target.PropertyChanged += (s, e) => eventWasRaisedCount++;

                gitExt.SetActiveRepository(repoInfo);
                gitExt.SetActiveRepository(repoInfo);

                Assert.That(1, Is.EqualTo(1));
            }

            [Test]
            public void ChangeActiveRepository()
            {
                var gitExt = new FakeGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = new GitRepositoryInfo(repositoryPath, null);
                var repositoryPath2 = Path.GetTempPath();
                var repoInfo2 = new GitRepositoryInfo(repositoryPath2, null);
                var target = CreateTeamExplorerContext(gitExt);
                gitExt.SetActiveRepository(repoInfo);
                var eventWasRaised = false;
                target.PropertyChanged += (s, e) => eventWasRaised = e.PropertyName == nameof(target.ActiveRepository);

                gitExt.SetActiveRepository(repoInfo2);

                Assert.That(eventWasRaised, Is.True);
            }

            [Test]
            public void ClearActiveRepository_NoEventWhenNoSolutionChange()
            {
                var gitExt = new FakeGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = new GitRepositoryInfo(repositoryPath, null);
                var target = CreateTeamExplorerContext(gitExt);
                gitExt.SetActiveRepository(repoInfo);
                var eventWasRaised = false;
                target.PropertyChanged += (s, e) => eventWasRaised = e.PropertyName == nameof(target.ActiveRepository);

                gitExt.SetActiveRepository(null);

                Assert.That(eventWasRaised, Is.False);
                Assert.That(target.ActiveRepository.LocalPath, Is.EqualTo(repositoryPath));
            }

            [Test]
            public void ClearActiveRepository_FireWhenSolutionChanged()
            {
                var gitExt = new FakeGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = new GitRepositoryInfo(repositoryPath, null);
                var dte = Substitute.For<DTE>();
                var target = CreateTeamExplorerContext(gitExt, dte);
                dte.Solution.FullName.Returns("Solution1");
                gitExt.SetActiveRepository(repoInfo);
                var eventWasRaised = false;
                target.PropertyChanged += (s, e) => eventWasRaised = e.PropertyName == nameof(target.ActiveRepository);

                dte.Solution.FullName.Returns("Solution2");
                gitExt.SetActiveRepository(null);

                Assert.That(eventWasRaised, Is.True);
                Assert.That(target.ActiveRepository, Is.Null);
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
                var gitExt = new FakeGitExt();
                var repositoryPaths = new[] { Directory.GetCurrentDirectory(), Path.GetTempPath() };
                var path1 = Directory.GetCurrentDirectory();
                var path2 = changePath ? Path.GetTempPath() : path1;
                var repoInfo1 = new GitRepositoryInfo(path1, new GitBranchInfo(name1, sha1));
                var repoInfo2 = new GitRepositoryInfo(path2, new GitBranchInfo(name2, sha2));
                var target = CreateTeamExplorerContext(gitExt);
                var eventWasRaised = false;
                target.StatusChanged += (s, e) => eventWasRaised = true;

                gitExt.SetActiveRepository(repoInfo1);
                gitExt.SetActiveRepository(repoInfo2);

                Assert.That(eventWasRaised, Is.EqualTo(expectWasRaised));
            }
        }

        static TeamExplorerContext CreateTeamExplorerContext(FakeGitExt gitExt, DTE dte = null)
        {
            var gitExtType = typeof(FakeGitExt);
            dte = dte ?? Substitute.For<DTE>();
            var sp = Substitute.For<IGitHubServiceProvider>();
            sp.GetService(gitExtType).Returns(gitExt);
            sp.TryGetService<DTE>().Returns(dte);
            var log = Substitute.For<ILogger>();
            return new TeamExplorerContext(sp, log, gitExtType);
        }

        class FakeGitExt : INotifyPropertyChanged
        {
            public void SetActiveRepository(GitRepositoryInfo repo)
            {
                ActiveRepositories = repo != null ? new[] { repo } : new GitRepositoryInfo[0];
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveRepositories)));
            }

            public IReadOnlyList<GitRepositoryInfo> ActiveRepositories { get; private set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        class GitRepositoryInfo
        {
            public GitRepositoryInfo(string repositoryPath, GitBranchInfo currentBranch)
            {
                RepositoryPath = repositoryPath;
                CurrentBranch = currentBranch;
            }

            public string RepositoryPath { get; }
            public GitBranchInfo CurrentBranch { get; }
        }

        class GitBranchInfo
        {
            public GitBranchInfo(string name, string headSha)
            {
                Name = name;
                HeadSha = headSha;
            }

            public string Name { get; }
            public string HeadSha { get; }
        }
    }
}
