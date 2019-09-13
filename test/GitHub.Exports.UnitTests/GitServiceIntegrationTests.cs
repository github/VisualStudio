using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Primitives;
using GitHub.Services;
using LibGit2Sharp;
using NUnit.Framework;

public class GitServiceIntegrationTests
{
    public class TheCompareMethod
    {
        [TestCase("1.2.", "1.2.3.4.", "+3.+4.", Description = "Two lines added")]
        public async Task Simple_Diff(string content1, string content2, string expectPatch)
        {
            using (var temp = new TempRepository())
            {
                var path = "foo.txt";
                var commit1 = AddCommit(temp.Repository, path, content1.Replace('.', '\n'));
                var commit2 = AddCommit(temp.Repository, path, content2.Replace('.', '\n'));
                var target = new GitService(new RepositoryFacade());

                var patch = await target.Compare(temp.Repository, commit1.Sha, commit2.Sha, path);

                Assert.That(patch.Content.Replace('\n', '.'), Contains.Substring(expectPatch));
            }
        }

        [TestCase("1.2.a..b.3.4", "1.2.a..b.a..b.3.4", "+b.+a.+.")] // This would be "+a.+.+b." without Indent-heuristic
        public async Task Indent_Heuristic_Is_Enabled(string content1, string content2, string expectPatch)
        {
            using (var temp = new TempRepository())
            {
                var path = "foo.txt";
                var commit1 = AddCommit(temp.Repository, path, content1.Replace('.', '\n'));
                var commit2 = AddCommit(temp.Repository, path, content2.Replace('.', '\n'));
                var target = new GitService(new RepositoryFacade());

                var patch = await target.Compare(temp.Repository, commit1.Sha, commit2.Sha, path);

                Assert.That(patch.Content.Replace('\n', '.'), Contains.Substring(expectPatch));
            }
        }

        [TestCase("foo", "bar")]
        public async Task One_File_Is_Modified(string content1, string content2)
        {
            using (var temp = new TempRepository())
            {
                var path = "foo.txt";
                var commit1 = AddCommit(temp.Repository, path, content1.Replace('.', '\n'));
                var commit2 = AddCommit(temp.Repository, path, content2.Replace('.', '\n'));
                var target = new GitService(new RepositoryFacade());

                var treeChanges = await target.Compare(temp.Repository, commit1.Sha, commit2.Sha, false);

                Assert.That(treeChanges.Modified.FirstOrDefault()?.Path, Is.EqualTo(path));
            }
        }


        [Test]
        public async Task Path_Can_Use_Windows_Directory_Separator()
        {
            using (var temp = new TempRepository())
            {
                var path = @"dir\foo.txt";
                var oldContent = "oldContent";
                var newContent = "newContent";
                var commit1 = AddCommit(temp.Repository, path, oldContent);
                var commit2 = AddCommit(temp.Repository, path, newContent);
                var target = new GitService(new RepositoryFacade());

                var patch = await target.Compare(temp.Repository, commit1.Sha, commit2.Sha, path);

                var gitPath = Paths.ToGitPath(path);
                Assert.That(patch.Count(c => c.Path == gitPath), Is.EqualTo(1));
            }
        }
    }

    public class TheCompareWithMethod
    {
        [TestCase("1.2.", "1.2.3.4.", "+3.+4.", Description = "Two lines added")]
        public async Task Simple_Diff(string content1, string content2, string expectPatch)
        {
            using (var temp = new TempRepository())
            {
                var path = "foo.txt";
                var commit1 = AddCommit(temp.Repository, path, content1.Replace('.', '\n'));
                var commit2 = AddCommit(temp.Repository, path, content2.Replace('.', '\n'));
                var contentBytes = new UTF8Encoding(false).GetBytes(content2.Replace('.', '\n'));
                var target = new GitService(new RepositoryFacade());

                var changes = await target.CompareWith(temp.Repository, commit1.Sha, commit2.Sha, path, contentBytes);

                Assert.That(changes.Patch.Replace('\n', '.'), Contains.Substring(expectPatch));
            }
        }

        [TestCase("1.2.a..b.3.4", "1.2.a..b.a..b.3.4", "+b.+a.+.")] // This would be "+a.+.+b." without Indent-heuristic
        public async Task Indent_Heuristic_Is_Enabled(string content1, string content2, string expectPatch)
        {
            using (var temp = new TempRepository())
            {
                var path = "foo.txt";
                var commit1 = AddCommit(temp.Repository, path, content1.Replace('.', '\n'));
                var commit2 = AddCommit(temp.Repository, path, content2.Replace('.', '\n'));
                var contentBytes = new UTF8Encoding(false).GetBytes(content2.Replace('.', '\n'));
                var target = new GitService(new RepositoryFacade());

                var changes = await target.CompareWith(temp.Repository, commit1.Sha, commit2.Sha, path, contentBytes);

                Assert.That(changes.Patch.Replace('\n', '.'), Contains.Substring(expectPatch));
            }
        }

        [TestCase("foo.txt", "a.b.", "bar.txt", "a.b.c.d.", 2)]
        [TestCase(@"dir/foo.txt", "a.b.", @"dir/bar.txt", "a.b.c.d.", 2)]
        [TestCase(@"dir/foo.txt", "a.b.", @"dir/foo.txt", "a.b.c.d.", 2)]
        [TestCase(@"dir/unrelated.txt", "x.x.x.x.", @"dir/foo.txt", "a.b.c.d.", 4)]
        public async Task Can_Handle_Renames(string oldPath, string oldContent, string newPath, string newContent, int expectLinesAdded)
        {
            using (var temp = new TempRepository())
            {
                var commit1 = AddCommit(temp.Repository, oldPath, oldContent.Replace('.', '\n'));
                var commit2 = AddCommit(temp.Repository, newPath, newContent.Replace('.', '\n'));
                var contentBytes = new UTF8Encoding(false).GetBytes(newContent.Replace('.', '\n'));
                var target = new GitService(new RepositoryFacade());

                var changes = await target.CompareWith(temp.Repository, commit1.Sha, commit2.Sha, newPath, contentBytes);

                Assert.That(changes?.LinesAdded, Is.EqualTo(expectLinesAdded));
            }
        }

        [Test]
        public async Task Path_Can_Use_Windows_Directory_Separator()
        {
            using (var temp = new TempRepository())
            {
                var path = @"dir\foo.txt";
                var oldContent = "oldContent";
                var newContent = "newContent";
                var commit1 = AddCommit(temp.Repository, path, oldContent);
                var commit2 = AddCommit(temp.Repository, path, newContent);
                var contentBytes = new UTF8Encoding(false).GetBytes(newContent);
                var target = new GitService(new RepositoryFacade());

                var contentChanges = await target.CompareWith(temp.Repository, commit1.Sha, commit2.Sha, path, contentBytes);

                Assert.That(contentChanges.LinesAdded, Is.EqualTo(1));
                Assert.That(contentChanges.LinesDeleted, Is.EqualTo(1));
            }
        }
    }

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

    static Commit AddCommit(Repository repo, string path = "file.txt", string content = null)
    {
        content = content ?? Guid.NewGuid().ToString();

        var dir = repo.Info.WorkingDirectory;
        DeleteFilesNotInGit(dir);
        var file = Path.Combine(dir, path);
        Directory.CreateDirectory(Path.GetDirectoryName(file));
        File.WriteAllText(file, content);
        Commands.Stage(repo, "*");
        var signature = new Signature("foobar", "foobar@github.com", DateTime.Now);
        var commit = repo.Commit("message", signature, signature);
        return commit;
    }

    static void DeleteFilesNotInGit(string dir)
    {
        var gitDir = Path.Combine(dir, @".git\");
        Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
            .Where(f => !f.StartsWith(gitDir, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .ForEach(File.Delete);
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
