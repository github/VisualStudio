using System;
using System.IO;
using System.Threading.Tasks;
using GitHub.Services;
using LibGit2Sharp;
using NUnit.Framework;

public class GitServiceIntegrationTests
{
    public class TheCreateLocalRepositoryModelMethod
    {
        [Test]
        public void Empty_Repository()
        {
            using (var temp = new TempRepository())
            {
                var path = temp.Directory.FullName;
                var target = new GitService(new RepositoryFacade());

                var model = target.CreateLocalRepositoryModel(path);

                Assert.That(model, Is.Not.Null);
                Assert.That(model.LocalPath, Is.EqualTo(path));
                Assert.That(model.Name, Is.EqualTo(temp.Directory.Name));
            }
        }

        [Test]
        public void No_Directory()
        {
            var unknownPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var target = new GitService(new RepositoryFacade());

            Assert.Throws<ArgumentException>(() => target.CreateLocalRepositoryModel(unknownPath));
        }

        [TestCase("origin", "https://github.com/github/VisualStudio", false)]
        [TestCase("not_origin", "https://github.com/github/VisualStudio", true)]
        [TestCase(null, null, false, Description = "Has no remotes")]
        public void Check_HasRemotesButNoOrigin(string remoteName, string remoteUrl, bool noOrigin)
        {
            using (var temp = new TempRepository())
            {
                if (remoteName != null)
                {
                    temp.Repository.Network.Remotes.Add(remoteName, remoteUrl);
                }
                var path = temp.Directory.FullName;
                var target = new GitService(new RepositoryFacade());

                var model = target.CreateLocalRepositoryModel(path);

                Assert.That(model.HasRemotesButNoOrigin, Is.EqualTo(noOrigin));
            }
        }

        [Test]
        public void NoRepository_Same_As_Repository_With_No_CloneUrl()
        {
            using (var temp = new TempDirectory())
            {
                var path = temp.Directory.FullName;
                var target = new GitService(new RepositoryFacade());

                var model = target.CreateLocalRepositoryModel(path);

                Assert.That(model, Is.Not.Null);
                Assert.That(model.LocalPath, Is.EqualTo(path));
                Assert.That(model.Name, Is.EqualTo(temp.Directory.Name));
            }
        }
    }

    public class TheGetBranchMethod
    {
        [Test]
        public void Master_Branch()
        {
            using (var temp = new TempRepository())
            {
                var signature = new Signature("Me", "my@email.com", DateTimeOffset.Now);
                temp.Repository.Commit("First", signature, signature);
                var expectSha = temp.Repository.Head.Tip.Sha;
                var path = temp.Directory.FullName;
                var target = new GitService(new RepositoryFacade());

                var localRepository = target.CreateLocalRepositoryModel(path);
                var branch = target.GetBranch(localRepository);

                Assert.That(branch.Name, Is.EqualTo("master"));
                Assert.That(branch.DisplayName, Is.EqualTo("master"));
                Assert.That(branch.Id, Is.EqualTo("/master")); // We don't know owner
                Assert.That(branch.IsTracking, Is.EqualTo(false));
                Assert.That(branch.TrackedSha, Is.EqualTo(null));
                Assert.That(branch.Sha, Is.EqualTo(expectSha));
            }
        }

        [Test]
        public void Branch_With_Remote()
        {
            using (var temp = new TempRepository())
            {
                var repository = temp.Repository;
                var owner = "owner";
                var remoteName = "remoteName";
                var remote = repository.Network.Remotes.Add(remoteName, $"https://github.com/{owner}/VisualStudio");
                var localBranch = repository.Head;
                repository.Branches.Update(temp.Repository.Head,
                    b => b.Remote = remote.Name,
                    b => b.UpstreamBranch = localBranch.CanonicalName);
                var path = temp.Directory.FullName;
                var target = new GitService(new RepositoryFacade());
                var localRepository = target.CreateLocalRepositoryModel(path);

                var branch = target.GetBranch(localRepository);

                Assert.That(branch.TrackedRemoteName, Is.EqualTo(remoteName));
            }
        }
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

                Assert.That(sha, Is.EqualTo(expectSha));
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

        [Test]
        public async Task TowPossibleRemoteBranches_ReturnNearestCommitSha()
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
                    var commit3 = AddCommit(repo);
                    var branch1 = repo.Branches.Add("branch1", commit1);
                    AddTrackedBranch(repo, branch1, commit1);
                    var branch2 = repo.Branches.Add("branch2", commit2);
                    AddTrackedBranch(repo, branch2, commit2);
                    expectSha = commit2.Sha;
                }

                var target = new GitService(new RepositoryFacade());

                var sha = await target.GetLatestPushedSha(dir).ConfigureAwait(false);

                Assert.That(sha, Is.EqualTo(expectSha));
            }
        }

        [TestCase("origin", true)]
        [TestCase("jcansdale", true, Description = "Search all remotes")]
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
    }

    protected class TempRepository : TempDirectory
    {
        public TempRepository()
            : base()
        {
            Repository = CreateRepository(Directory.FullName);
        }

        static Repository CreateRepository(string path)
        {
            return new Repository(Repository.Init(path));
        }

        public Repository Repository
        {
            get;
        }
    }

    protected class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            var f = Path.GetTempFileName();
            var name = Path.GetFileNameWithoutExtension(f);
            File.Delete(f);
            Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), name));
            Directory.Create();
        }

        public DirectoryInfo Directory { get; }

        public void Dispose()
        {
            // Remove any read-only attributes
            SetFileAttributes(Directory, FileAttributes.Normal);
            Directory.Delete(true);
        }

        static void SetFileAttributes(DirectoryInfo dir, FileAttributes attributes)
        {
            foreach (DirectoryInfo subdir in dir.GetDirectories())
            {
                SetFileAttributes(subdir, attributes);
            }

            foreach (var file in dir.GetFiles())
            {
                File.SetAttributes(file.FullName, attributes);
            }
        }
    }
}
