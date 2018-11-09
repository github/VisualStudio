using System;
using System.IO;
using GitHub.Services;
using NUnit.Framework;
using NSubstitute;
using EnvDTE;
using GitHub.Models;
using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;

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
            public async Task SetActiveRepository_CheckWasSet()
            {
                var gitExt = CreateGitExt();
                var repositoryPath = Directory.GetCurrentDirectory();
                var repoInfo = CreateRepositoryModel(repositoryPath);
                SetActiveRepository(gitExt, repoInfo);
                var target = CreateTeamExplorerContext(gitExt);

                await target.JoinableTaskCollection.JoinTillEmptyAsync();

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
            [TestCase("path", "path", true)]
            [TestCase("path1", "path2", false)]
            [TestCase(null, null, false)]
            public void AlwaysFireWhenNoLocalPathChange(string path1, string path2, bool expectWasRaised)
            {
                var gitExt = CreateGitExt();
                var repositoryPaths = new[] { Directory.GetCurrentDirectory(), Path.GetTempPath() };
                var repoInfo1 = CreateRepositoryModel(path1);
                var repoInfo2 = CreateRepositoryModel(path2);

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
                var repoInfo1 = CreateRepositoryModel(path);
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

        static TeamExplorerContext CreateTeamExplorerContext(
            IVSGitExt gitExt,
            DTE dte = null,
            IPullRequestService pullRequestService = null,
            JoinableTaskContext joinableTaskContext = null)
        {
            dte = dte ?? Substitute.For<DTE>();
            pullRequestService = pullRequestService ?? Substitute.For<IPullRequestService>();
            joinableTaskContext = joinableTaskContext ?? new JoinableTaskContext();
            return new TeamExplorerContext(gitExt, new AsyncLazy<DTE>(() => Task.FromResult(dte)), pullRequestService, joinableTaskContext);
        }

        static LocalRepositoryModel CreateRepositoryModel(string path)
        {
            return new LocalRepositoryModel
            {
                LocalPath = path
            };
        }

        static IVSGitExt CreateGitExt()
        {
            return Substitute.For<IVSGitExt>();
        }

        static void SetActiveRepository(IVSGitExt gitExt, LocalRepositoryModel repo)
        {
            var repos = repo != null ? new[] { repo } : Array.Empty<LocalRepositoryModel>();
            gitExt.ActiveRepositories.Returns(repos);
            gitExt.ActiveRepositoriesChanged += Raise.Event<Action>();
        }
    }
}
