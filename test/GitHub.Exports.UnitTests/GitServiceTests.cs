using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using NUnit.Framework;

public class GitServiceTests
{
    [TestCase("asdf", null)]
    [TestCase("", null)]
    [TestCase(null, null)]
    [TestCase("file:///C:/dev/exp/foo", "file:///C:/dev/exp/foo")]
    [TestCase("http://example.com/", "http://example.com/")]
    [TestCase("http://haacked@example.com/foo/bar", "http://example.com/foo/bar")]
    [TestCase("https://github.com/github/Windows", "https://github.com/github/Windows")]
    [TestCase("https://github.com/github/Windows.git", "https://github.com/github/Windows")]
    [TestCase("https://haacked@github.com/github/Windows.git", "https://github.com/github/Windows")]
    [TestCase("http://example.com:4000/github/Windows", "http://example.com:4000/github/Windows")]
    [TestCase("git@192.168.1.2:github/Windows.git", "https://192.168.1.2/github/Windows")]
    [TestCase("git@example.com:org/repo.git", "https://example.com/org/repo")]
    [TestCase("ssh://git@github.com:443/shana/cef", "https://github.com/shana/cef")]
    [TestCase("ssh://git@example.com:23/haacked/encourage", "https://example.com:23/haacked/encourage")]
    public void GetUriShouldNotThrow(string url, string expected)
    {
        var origin = Substitute.For<Remote>();
        origin.Url.Returns(url);
        var path = "path";
        var repository = CreateRepository();
        repository.Network.Remotes["origin"].Returns(origin);
        var repositoryFacade = CreateRepositoryFacade(path, repository);
        var target = new GitService(repositoryFacade);

        var repositoryUrl = target.GetUri(repository)?.ToString();

        Assert.That(expected, Is.EqualTo(repositoryUrl));
    }

    static IRepositoryFacade CreateRepositoryFacade(string path, IRepository repo)
    {
        var repositoryFacade = Substitute.For<IRepositoryFacade>();
        repositoryFacade.Discover(path).Returns(path);
        repositoryFacade.NewRepository(path).Returns(repo);
        return repositoryFacade;
    }

    static IRepository CreateRepository()
    {
        var repo = Substitute.For<IRepository>();
        return repo;
    }

    public class TheGetLatestPushedShaMethod : TestBaseClass
    {
        [Test]
        public async Task EmptyRepository_ReturnsNull()
        {
            using (var temp = new TempDirectory())
            {
                string expectSha;
                var dir = temp.Directory.FullName;
                using (var repo = new Repository(Repository.Init(dir)))
                {
                    expectSha = null;
                }

                var target = new GitService(new RepositoryFacade());

                var sha = await target.GetLatestPushedSha(dir).ConfigureAwait(false);

                Assert.That(sha, Is.Null);
            }
        }

        [Test]
        public async Task HeadAndRemoteOnSameCommit_ReturnCommitSha()
        {
            using (var temp = new TempDirectory())
            {
                string expectSha;
                var dir = temp.Directory.FullName;
                using (var repo = new Repository(Repository.Init(dir)))
                {
                    AddCommit(repo); // First commit
                    var commit = AddCommit(repo);
                    expectSha = commit.Sha;
                    AddTrackedBranch(repo, repo.Head, commit);
                }

                var target = new GitService(new RepositoryFacade());

                var sha = await target.GetLatestPushedSha(dir).ConfigureAwait(false);

                Assert.That(sha, Is.EqualTo(expectSha));
            }
        }

        [Test]
        public async Task LocalAheadOfRemote_ReturnRemoteCommitSha()
        {
            using (var temp = new TempDirectory())
            {
                string expectSha;
                var dir = temp.Directory.FullName;
                using (var repo = new Repository(Repository.Init(dir)))
                {
                    AddCommit(repo); // First commit
                    var commit = AddCommit(repo);
                    expectSha = commit.Sha;
                    AddTrackedBranch(repo, repo.Head, commit);
                    AddCommit(repo);
                }

                var target = new GitService(new RepositoryFacade());

                var sha = await target.GetLatestPushedSha(dir).ConfigureAwait(false);

                Assert.That(sha, Is.EqualTo(expectSha));
            }
        }

        [Test]
        public async Task LocalBehindRemote_ReturnRemoteCommitSha()
        {
            using (var temp = new TempDirectory())
            {
                string expectSha;
                var dir = temp.Directory.FullName;
                using (var repo = new Repository(Repository.Init(dir)))
                {
                    AddCommit(repo); // First commit
                    var commit1 = AddCommit(repo);
                    var commit2 = AddCommit(repo);
                    repo.Reset(ResetMode.Hard, commit1);
                    expectSha = commit1.Sha;
                    AddTrackedBranch(repo, repo.Head, commit2);
                }

                var target = new GitService(new RepositoryFacade());

                var sha = await target.GetLatestPushedSha(dir).ConfigureAwait(false);

                Assert.That(sha, Is.EqualTo(expectSha));
            }
        }

        [Test]
        public async Task BranchForkedFromMaster_ReturnRemoteCommitSha()
        {
            using (var temp = new TempDirectory())
            {
                string expectSha;
                var dir = temp.Directory.FullName;
                using (var repo = new Repository(Repository.Init(dir)))
                {
                    AddCommit(repo); // First commit
                    var commit1 = AddCommit(repo);
                    AddTrackedBranch(repo, repo.Head, commit1);
                    var branch = repo.Branches.Add("branch", commit1);
                    Commands.Checkout(repo, branch);
                    var commit2 = AddCommit(repo);
                    expectSha = commit1.Sha;
                }

                var target = new GitService(new RepositoryFacade());

                var sha = await target.GetLatestPushedSha(dir).ConfigureAwait(false);

                Assert.That(sha, Is.EqualTo(expectSha));
            }
        }

        [TestCase("origin", true)]
        [TestCase("jcansdale", false)]
        public async Task BehindRemoteBranch_ReturnRemoteCommitSha(string remoteName, bool expectFound)
        {
            using (var temp = new TempDirectory())
            {
                string expectSha;
                var dir = temp.Directory.FullName;
                using (var repo = new Repository(Repository.Init(dir)))
                {
                    AddCommit(repo); // First commit
                    var commit1 = AddCommit(repo);
                    var commit2 = AddCommit(repo);
                    var branchA = repo.Branches.Add("branchA", commit2);
                    repo.Reset(ResetMode.Hard, commit1);
                    expectSha = expectFound ? commit1.Sha : null;
                    AddTrackedBranch(repo, branchA, commit2, remoteName: remoteName);
                }

                var target = new GitService(new RepositoryFacade());

                var sha = await target.GetLatestPushedSha(dir).ConfigureAwait(false);

                Assert.That(sha, Is.EqualTo(expectSha));
            }
        }

        [TestCase("refs/remotes/origin/master", true)]
        [TestCase("refs/pull/777/head", true)]
        [TestCase("refs/heads/other", false)]
        [TestCase("refs/remotes/other/master", false)]
        public async Task BehindRefWithCanonicalName_ReturnRemoteCommitSha(string canonicalName, bool expectFound)
        {
            using (var temp = new TempDirectory())
            {
                string expectSha;
                var dir = temp.Directory.FullName;
                using (var repo = new Repository(Repository.Init(dir)))
                {
                    AddCommit(repo); // First commit
                    var commit1 = AddCommit(repo);
                    var commit2 = AddCommit(repo);
                    repo.Reset(ResetMode.Hard, commit1);
                    expectSha = expectFound ? commit1.Sha : null;
                    repo.Refs.Add(canonicalName, commit2.Id);
                }

                var target = new GitService(new RepositoryFacade());

                var sha = await target.GetLatestPushedSha(dir).ConfigureAwait(false);

                Assert.That(sha, Is.EqualTo(expectSha));
            }
        }

        static Commit AddCommit(Repository repo)
        {
            var dir = repo.Info.WorkingDirectory;
            var path = "file.txt";
            var file = Path.Combine(dir, path);
            var guidString = Guid.NewGuid().ToString();
            File.WriteAllText(file, guidString);
            Commands.Stage(repo, path);
            var signature = new Signature("foobar", "foobar@github.com", DateTime.Now);
            var commit = repo.Commit("message", signature, signature);
            return commit;
        }

        static void AddTrackedBranch(Repository repo, Branch branch, Commit commit,
            string trackedBranchName = null, string remoteName = "origin")
        {
            trackedBranchName = trackedBranchName ?? branch.FriendlyName;

            if (repo.Network.Remotes[remoteName] == null)
            {
                repo.Network.Remotes.Add(remoteName, "https://github.com/owner/repo");
            }
            var canonicalName = $"refs/remotes/{remoteName}/{trackedBranchName}";
            repo.Refs.Add(canonicalName, commit.Id);
            repo.Branches.Update(branch, b => b.TrackedBranch = canonicalName);
        }

        public async Task IntergrationTest()
        {
            var path = @"C:\Source\github.com\dotnet\roslyn";
            var gitService = new GitService(new RepositoryFacade());
            var sha = await gitService.GetLatestPushedSha(path);
            Console.WriteLine(sha);
        }
    }
}
