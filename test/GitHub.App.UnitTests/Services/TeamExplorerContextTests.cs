using System;
using System.Collections.Generic;
using GitHub.Services;
using NUnit.Framework;
using NSubstitute;
using System.ComponentModel;
using System.IO;

namespace GitHub.App.UnitTests.Services
{
    public class TeamExplorerContextTests
    {
        public class TheActiveRepositoryProperty
        {
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

        static TeamExplorerContext CreateTeamExplorerContext(FakeGitExt gitExt)
        {
            var gitExtType = typeof(FakeGitExt);
            var sp = Substitute.For<IServiceProvider>();
            sp.GetService(gitExtType).Returns(gitExt);
            return new TeamExplorerContext(sp, gitExtType, true);
        }

        class FakeGitExt : INotifyPropertyChanged
        {
            public void SetActiveRepository(GitRepositoryInfo repo)
            {
                ActiveRepositories = new[] { repo };
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
