using System;
using System.Linq;
using GitHub.Exports;
using GitHub.Services;
using NSubstitute;
using NUnit.Framework;
using LibGit2Sharp;

public class GitHubContextServiceTests
{
    public class TheFindContextFromUrlMethod
    {
        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github", "github")]
        [TestCase("https://github.com/github/VisualStudio", "github")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", "github")]
        public void Owner(string url, string expectOwner)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
        }

        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github", null)]
        [TestCase("https://github.com/github/VisualStudio", "VisualStudio")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", "VisualStudio")]
        public void RepositoryName(string url, string expectRepositoryName)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
        }

        [TestCase("https://github.com", "github.com")]
        [TestCase("https://github.com/github", "github.com")]
        [TestCase("https://github.com/github/VisualStudio", "github.com")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", "github.com")]
        public void Host(string url, string expectHost)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Host, Is.EqualTo(expectHost));
        }

        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github", null)]
        [TestCase("https://github.com/github/VisualStudio", null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", null)]
        [TestCase("https://github.com/github/VisualStudio/pull/1763", 1763)]
        [TestCase("https://github.com/github/VisualStudio/pull/1763/commits", 1763)]
        [TestCase("https://github.com/github/VisualStudio/pull/1763/files#diff-7384294e6c288e13bad0293bae232754R1", 1763)]
        [TestCase("https://github.com/github/VisualStudio/pull/NaN", null)]
        public void PullRequest(string url, int? expectPullRequest)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context?.PullRequest, Is.EqualTo(expectPullRequest));
        }

        [TestCase("https://github.com/github/VisualStudio/blob/master", null, null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/foo.cs", "master", "foo.cs")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/path/foo.cs", "master/path", "foo.cs")]
        [TestCase("https://github.com/github/VisualStudio/blob/ee863ce265fc6217f589e66766125fed1b5b8256/foo.cs", "ee863ce265fc6217f589e66766125fed1b5b8256", "foo.cs")]
        [TestCase("https://github.com/github/VisualStudio/blob/ee863ce265fc6217f589e66766125fed1b5b8256/path/foo.cs", "ee863ce265fc6217f589e66766125fed1b5b8256/path", "foo.cs")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/bar.cs#stuff", "master", "bar.cs")]
        public void Blob(string url, string expectTreeishPath, string expectBlobName)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.TreeishPath, Is.EqualTo(expectTreeishPath));
            Assert.That(context.BlobName, Is.EqualTo(expectBlobName));
        }

        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github", null)]
        [TestCase("https://github.com/github/VisualStudio", null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md#notices", null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/src/GitHub.VisualStudio/GitHubPackage.cs#L38", 38)]
        [TestCase("https://github.com/github/VisualStudio/blob/0d264d50c57d701fa62d202f481075a6c6dbdce8/src/Code.cs#L86", 86)]
        public void Line(string url, int? expectLine)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Line, Is.EqualTo(expectLine));
        }

        [TestCase("https://github.com/github/VisualStudio", null, null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/Code.cs#L115", 115, null)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/Code.cs#L115-L116", 115, 116)]
        public void LineEnd(string url, int? expectLine, int? expectLineEnd)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Line, Is.EqualTo(expectLine));
            Assert.That(context.LineEnd, Is.EqualTo(expectLineEnd));
        }

        [TestCase("foo", true)]
        [TestCase("ssh://git@github.com:443/benstraub/libgit2", true)]
        [TestCase("https://github.com/github/VisualStudio", false)]
        public void IsNull(string url, bool expectNull)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context, expectNull ? Is.Null : Is.Not.Null);
        }

        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", "https://github.com/github/VisualStudio/blob/master/README.md")]
        public void Url_EqualTo(string url, string expectUrl)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Url?.ToString(), Is.EqualTo(expectUrl));
        }

        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", LinkType.Blob)]
        [TestCase("https://github.com/github/VisualStudio/unknown/master/README.md", LinkType.Unknown)]
        public void LinkType_EqualTo(string url, LinkType expectLinkType)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.LinkType, Is.EqualTo(expectLinkType));
        }
    }

    public class TheToMethod
    {
        [Test]
        public void DefaultGitHubDotCom()
        {
            var context = new GitHubContext { Host = "github.com", Owner = "github", RepositoryName = "VisualStudio" };
            var target = CreateGitHubContextService();

            var uri = target.ToRepositoryUrl(context);

            Assert.That(uri, Is.EqualTo(new Uri("https://github.com/github/VisualStudio")));
        }
    }

    public class TheFindContextFromWindowTitleMethod
    {
        [TestCase("github/0123456789: Description - Google Chrome", "0123456789")]
        [TestCase("github/abcdefghijklmnopqrstuvwxyz: Description - Google Chrome", "abcdefghijklmnopqrstuvwxyz")]
        [TestCase("github/ABCDEFGHIJKLMNOPQRSTUVWXYZ: Description - Google Chrome", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("github/_: Description - Google Chrome", "_")]
        [TestCase("github/.: Description - Google Chrome", ".")]
        [TestCase("github/-: Description - Google Chrome", "-")]
        [TestCase("github/$: Description - Google Chrome", null, Description = "Must contain only letters, numbers, `_`, `.` or `-`")]
        public void RepositoryName(string windowTitle, string expectRepositoryName)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.RepositoryName, Is.EqualTo(expectRepositoryName));
        }

        [TestCase("0123456789/Repository: Description - Google Chrome", "0123456789")]
        [TestCase("abcdefghijklmnopqrstuvwxyz/Repository: Description - Google Chrome", "abcdefghijklmnopqrstuvwxyz")]
        [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZ/Repository: Description - Google Chrome", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [TestCase("a_/Repository: Description - Google Chrome", "a_")]
        [TestCase("a-/Repository: Description - Google Chrome", "a-")]
        [TestCase("_/Repository: Description - Google Chrome", null, Description = "Must start with letter or number")]
        [TestCase("-/Repository: Description - Google Chrome", null, Description = "Must start with letter or number")]
        public void Owner(string windowTitle, string expectOwner)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.Owner, Is.EqualTo(expectOwner));
        }

        // They can include slash / for hierarchical (directory) grouping
        [TestCase("a/b", "a/b", Description = "")]
        [TestCase("aaa/bbb", "aaa/bbb", Description = "")]

        // They cannot have space, tilde ~, caret ^, or colon : anywhere.
        [TestCase("a b", null)]
        [TestCase("a~b", null)]
        [TestCase("a^b", null)]
        [TestCase("a:b", null)]

        // They cannot have question-mark ?, asterisk *, or open bracket [ anywhere.
        [TestCase("a?b", null)]
        [TestCase("a*b", null)]
        [TestCase("a[b", null)]

        [TestCase(@"a\b", null, Description = @"They cannot contain a \")]

        // Simple case
        [TestCase("master", "master")]

        // There are many symbols they can contain
        [TestCase("!@#$%&()_+-=", "!@#$%&()_+-=")]

        [TestCase("/a", null, Description = "They cannot begin a slash")]
        [TestCase("a/", null, Description = "They cannot end with a slash")]
        [TestCase("../b", null, Description = "no slash-separated component can begin with a dot")]
        [TestCase(".a/b", null, Description = "no slash-separated component can begin with a dot")]
        [TestCase("a/.b", null, Description = "no slash-separated component can begin with a dot")]

        // There are some checks we aren't doing, see https://git-scm.com/docs/git-check-ref-format
        // They cannot have ASCII control characters(i.e.bytes whose values are lower than \040, or \177 DEL)        
        // [TestCase("a/b.lock", null, Description = "or end with the sequence.lock")]
        // [TestCase("a..b", null, Description = "They cannot have two consecutive dots..anywhere")]
        // [TestCase("a.", null, Description = "They cannot end with a dot")]
        // [TestCase("@{a", null, Description = "They cannot contain a sequence @{")]
        // [TestCase("@", null, Description = "They cannot be the single character @")]
        public void Branch(string branch, string expectBranch)
        {
            var windowTitle = $"VisualStudio/src/GitHub.VisualStudio/Resources/icons at {branch} · github/VisualStudio - Google Chrome";
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.BranchName, Is.EqualTo(expectBranch));
        }

        [TestCase("github/VisualStudio: GitHub Extension for Visual Studio - Google Chrome", "github", "VisualStudio", null)]
        [TestCase("Branches · github/VisualStudio - Google Chrome", "github", "VisualStudio", null)]
        [TestCase("github/VisualStudio at build/appveyor-fixes - Google Chrome", "github", "VisualStudio", "build/appveyor-fixes")]
        [TestCase("[spike] Open from GitHub URL by jcansdale · Pull Request #1763 · github/VisualStudio - Google Chrome", "github", "VisualStudio", null)]
        [TestCase("Consider adding C# code style preferences to editorconfig · Issue #1750 · github/VisualStudio - Google Chrome", "github", "VisualStudio", null)]
        [TestCase("VisualStudio/mark_github.xaml at master · github/VisualStudio - Google Chrome", "github", "VisualStudio", "master")]
        [TestCase("VisualStudio/src/GitHub.VisualStudio/Resources/icons at master · github/VisualStudio - Google Chrome", "github", "VisualStudio", "master")]
        [TestCase("VisualStudio/GitHub.Exports.csproj at 89484dc25a3a475d3253afdc3bd3ddd6c6999c3b · github/VisualStudio - Google Chrome", "github", "VisualStudio", "89484dc25a3a475d3253afdc3bd3ddd6c6999c3b")]
        public void OwnerRepositoryBranch(string windowTitle, string expectOwner, string expectRepositoryName, string expectBranch)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
            Assert.That(context.BranchName, Is.EqualTo(expectBranch));
        }

        [TestCase("github/VisualStudio at build/appveyor-fixes - Google Chrome", "github", "VisualStudio", "build/appveyor-fixes", Description = "Chrome")]
        [TestCase("GitHub - github/VisualStudio at refactor/pr-list - Mozilla Firefox", "github", "VisualStudio", "refactor/pr-list", Description = "Firefox")]
        public void TreeBranch(string windowTitle, string expectOwner, string expectRepositoryName, string expectBranch)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
            Assert.That(context.BranchName, Is.EqualTo(expectBranch));
        }

        [TestCase("Branches · github/VisualStudio - Google Chrome", "github", "VisualStudio", Description = "Chrome")]
        [TestCase("Branches · github/VisualStudio · GitHub - Mozilla Firefox", "github", "VisualStudio", Description = "Firefox")]
        public void Branches(string windowTitle, string expectOwner, string expectRepositoryName)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
        }

        [TestCase("Description · Pull Request #1763 · github/VisualStudio - Google Chrome", 1763)]
        [TestCase("Description · Pull Request #1763 · github/VisualStudio · GitHub - Mozilla Firefox", 1763, Description = "Firefox")]
        public void PullRequest(string windowTitle, int expectPullRequest)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.PullRequest, Is.EqualTo(expectPullRequest));
        }

        [TestCase("Consider adding C# code style preferences to editorconfig · Issue #1750 · github/VisualStudio - Google Chrome", 1750)]
        [TestCase("Scrape browser titles · Issue #4 · jcansdale/VisualStudio · GitHub - Mozilla Firefox", 4, Description = "Firefox")]
        public void Issue(string windowTitle, int expectIssue)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Issue, Is.EqualTo(expectIssue));
        }

        [TestCase("VisualStudio/mark_github.xaml at master · github/VisualStudio - Google Chrome", "mark_github.xaml", "master")]
        [TestCase("VisualStudio/src/GitHub.VisualStudio/Resources/icons at master · github/VisualStudio - Google Chrome", null, "master")]
        [TestCase("VisualStudio/src at master · github/VisualStudio - Google Chrome", "src", "master", Description = "Can't differentiate between single level tree and blob")]
        [TestCase("VisualStudio/README.md at master · jcansdale/VisualStudio · GitHub - Mozilla Firefox", "README.md", "master", Description = "Firefox")]
        public void Blob(string windowTitle, string expectBlobName, string expectBranchName)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.BlobName, Is.EqualTo(expectBlobName));
            Assert.That(context?.BranchName, Is.EqualTo(expectBranchName));
        }

        [TestCase("VisualStudio/src/GitHub.VisualStudio/Resources/icons at master · github/VisualStudio - Google Chrome", "master/src/GitHub.VisualStudio/Resources/icons", "master")]
        public void Tree(string windowTitle, string expectTreeish, string expectBranch)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.TreeishPath, Is.EqualTo(expectTreeish));
            Assert.That(context?.BranchName, Is.EqualTo(expectBranch));
        }

        [TestCase("jcansdale/VisualStudio: GitHub Extension for Visual Studio - Google Chrome", "jcansdale", "VisualStudio", Description = "Chrome")]
        [TestCase("GitHub - jcansdale/VisualStudio: GitHub Extension for Visual Studio - Mozilla Firefox", "jcansdale", "VisualStudio", Description = "Firefox")]
        [TestCase("jcansdale/GhostAssemblies - Google Chrome", "jcansdale", "GhostAssemblies", Description = "No description, Chrome")]
        [TestCase("GitHub - jcansdale/GhostAssemblies - Mozilla Firefox", "jcansdale", "GhostAssemblies", Description = "No description, Firefox")]
        public void RepositoryHome(string windowTitle, string expectOwner, string expectRepositoryName)
        {
            var target = CreateGitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.Owner, Is.EqualTo(expectOwner));
            Assert.That(context?.RepositoryName, Is.EqualTo(expectRepositoryName));
        }
    }

    public class TheResolveBlobMethod
    {
        const string CommitSha = "36d6b0bb6e319337180d523281c42d9611744e66";

        [TestCase("https://github.com/github/VisualStudio/blob/master/foo.cs", "refs/remotes/origin/master", "refs/remotes/origin/master:foo.cs", "refs/remotes/origin/master", "foo.cs", CommitSha)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/src/foo.cs", "refs/remotes/origin/master", "refs/remotes/origin/master:src/foo.cs", "refs/remotes/origin/master", "src/foo.cs", CommitSha)]
        [TestCase("https://github.com/github/VisualStudio/blob/branch-name/src/foo.cs", "refs/remotes/origin/branch-name", "refs/remotes/origin/branch-name:src/foo.cs", "refs/remotes/origin/branch-name", "src/foo.cs", CommitSha)]
        [TestCase("https://github.com/github/VisualStudio/blob/fixes/666-bug/src/foo.cs", "refs/remotes/origin/fixes/666-bug", "refs/remotes/origin/fixes/666-bug:src/foo.cs", "refs/remotes/origin/fixes/666-bug", "src/foo.cs", CommitSha)]
        [TestCase("https://github.com/github/VisualStudio/blob/fixes/666-bug/A/B/foo.cs", "refs/remotes/origin/fixes/666-bug", "refs/remotes/origin/fixes/666-bug:A/B/foo.cs", "refs/remotes/origin/fixes/666-bug", "A/B/foo.cs", CommitSha)]
        [TestCase("https://github.com/github/VisualStudio/blob/master/foo.cs", "refs/remotes/origin/master", null, "refs/remotes/origin/master", null, CommitSha, Description = "Resolve commit only")]
        [TestCase("https://github.com/github/VisualStudio/blob/36d6b0bb6e319337180d523281c42d9611744e66/src/code.cs", CommitSha, CommitSha + ":src/code.cs", CommitSha, "src/code.cs", CommitSha, Description = "Resolve commit only")]
        [TestCase("https://github.com/github/VisualStudio/commit/8cf9a268c497adb4fc0a14572253165e179dd11e", "8cf9a268c497adb4fc0a14572253165e179dd11e", null, null, null, null)]
        [TestCase("https://github.com/github/VisualStudio/blob/v2.5.3.2888/build.cmd", "refs/tags/v2.5.3.2888", "refs/tags/v2.5.3.2888:build.cmd", "refs/tags/v2.5.3.2888", "build.cmd", CommitSha)]
        public void ResolveBlob(string url, string commitish, string objectish, string expectCommitish, string expectPath, string expectCommitSha)
        {
            var repositoryDir = "repositoryDir";
            var repository = Substitute.For<IRepository>();
            var commit = Substitute.For<Commit>();
            commit.Sha.Returns(expectCommitSha);
            var blob = Substitute.For<Blob>();
            repository.Lookup(commitish).Returns(commit);
            repository.Lookup(objectish).Returns(blob);
            if (ObjectId.TryParse(commitish, out ObjectId objectId))
            {
                // If it looks like a SHA, allow lookup using its ObjectId
                repository.Lookup(objectId).Returns(blob);
            }
            var target = CreateGitHubContextService(repositoryDir, repository);
            var context = target.FindContextFromUrl(url);

            var (resolvedCommitish, resolvedPath, commitSha) = target.ResolveBlob(repositoryDir, context);

            Assert.That(resolvedCommitish, Is.EqualTo(expectCommitish));
            Assert.That(resolvedPath, Is.EqualTo(expectPath));
            Assert.That(commitSha, Is.EqualTo(expectCommitSha));
        }
    }

    public class TheResolveBlobFromCommitsMethod
    {
        [Test]
        public void FlatTree()
        {
            var objectish = "12345678";
            var expectCommitSha = "2434215c5489db2bfa2e5249144a3bc532465f97";
            var expectBlobPath = "Class1.cs";
            var repositoryDir = "repositoryDir";
            var blob = Substitute.For<Blob>();
            var treeEntry = CreateTreeEntry(TreeEntryTargetType.Blob, blob, expectBlobPath);
            var commit = CreateCommit(expectCommitSha, treeEntry);
            var repository = CreateRepository(commit);
            repository.Lookup<Blob>(objectish).Returns(blob);
            var target = CreateGitHubContextService(repositoryDir, repository);

            var (commitSha, blobPath) = target.ResolveBlobFromHistory(repositoryDir, objectish);

            Assert.That((commitSha, blobPath), Is.EqualTo((expectCommitSha, expectBlobPath)));
        }

        [Test]
        public void NestedTree()
        {
            var objectish = "12345678";
            var expectCommitSha = "2434215c5489db2bfa2e5249144a3bc532465f97";
            var expectBlobPath = @"AnnotateFileTests\Class1.cs";
            var repositoryDir = "repositoryDir";
            var blob = Substitute.For<Blob>();
            var blobTreeEntry = CreateTreeEntry(TreeEntryTargetType.Blob, blob, expectBlobPath);
            var childTree = CreateTree(blobTreeEntry);
            var treeTreeEntry = CreateTreeEntry(TreeEntryTargetType.Tree, childTree, "AnnotateFileTests");
            var commit = CreateCommit(expectCommitSha, treeTreeEntry);
            var repository = CreateRepository(commit);
            repository.Lookup<Blob>(objectish).Returns(blob);
            var target = CreateGitHubContextService(repositoryDir, repository);

            var (commitSha, blobPath) = target.ResolveBlobFromHistory(repositoryDir, objectish);

            Assert.That((commitSha, blobPath), Is.EqualTo((expectCommitSha, expectBlobPath)));
        }

        [Test]
        public void MissingBlob()
        {
            var objectish = "12345678";
            var repositoryDir = "repositoryDir";
            var treeEntry = Substitute.For<TreeEntry>();
            var repository = CreateRepository();
            var target = CreateGitHubContextService(repositoryDir, repository);

            var (commitSha, blobPath) = target.ResolveBlobFromHistory(repositoryDir, objectish);

            Assert.That((commitSha, blobPath), Is.EqualTo((null as string, null as string)));
        }

        static IRepository CreateRepository(params Commit[] commits)
        {
            var repository = Substitute.For<IRepository>();
            var enumerator = commits.ToList().GetEnumerator();
            repository.Commits.GetEnumerator().Returns(enumerator);
            return repository;
        }

        static Commit CreateCommit(string sha, params TreeEntry[] treeEntries)
        {
            var commit = Substitute.For<Commit>();
            commit.Sha.Returns(sha);
            var tree = CreateTree(treeEntries);
            commit.Tree.Returns(tree);
            return commit;
        }

        static TreeEntry CreateTreeEntry(TreeEntryTargetType targetType, GitObject target, string path)
        {
            var treeEntry = Substitute.For<TreeEntry>();
            treeEntry.TargetType.Returns(targetType);
            treeEntry.Target.Returns(target);
            treeEntry.Path.Returns(path);
            return treeEntry;
        }

        static Tree CreateTree(params TreeEntry[] treeEntries)
        {
            var tree = Substitute.For<Tree>();
            var enumerator = treeEntries.ToList().GetEnumerator();
            tree.GetEnumerator().Returns(enumerator);
            return tree;
        }
    }

    public class TheFindBlobShaForTextViewMethod
    {
        [TestCase(@"C:\Users\me\AppData\Local\Temp\TFSTemp\vctmp21996_181282.IOpenFromClipboardCommand.783ac965.cs", "783ac965")]
        [TestCase(@"\TFSTemp\File.12345678.ext", "12345678")]
        [TestCase(@"\TFSTemp\File.abcdefab.ext", "abcdefab")]
        [TestCase(@"\TFSTemp\.12345678.", "12345678")]
        [TestCase(@"\TFSTemp\File.ABCDEFAB.ext", null)]
        [TestCase(@"\TFSTemp\File.1234567.ext", null)]
        [TestCase(@"\TFSTemp\File.123456789.ext", null)]
        [TestCase(@"\TFSTemp\File.12345678.ext\\", null)]
        public void FindObjectishForTFSTempFile(string path, string expectObjectish)
        {
            var target = CreateGitHubContextService();

            var objectish = target.FindObjectishForTFSTempFile(path);

            Assert.That(objectish, Is.EqualTo(expectObjectish));
        }
    }

    static GitHubContextService CreateGitHubContextService(string repositoryDir = null, IRepository repository = null)
    {
        var sp = Substitute.For<IGitHubServiceProvider>();
        var gitService = Substitute.For<IGitService>();
        var vsServices = Substitute.For<IVSServices>();
        gitService.GetRepository(repositoryDir).Returns(repository);

        return new GitHubContextService(sp, gitService, vsServices);
    }
}
