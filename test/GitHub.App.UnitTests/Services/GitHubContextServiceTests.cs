using GitHub.App.Services;
using NUnit.Framework;

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
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
        }

        [TestCase("https://github.com", null)]
        [TestCase("https://github.com/github", null)]
        [TestCase("https://github.com/github/VisualStudio", "VisualStudio")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", "VisualStudio")]
        public void RepositoryName(string url, string expectRepositoryName)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
        }

        [TestCase("https://github.com", "github.com")]
        [TestCase("https://github.com/github", "github.com")]
        [TestCase("https://github.com/github/VisualStudio", "github.com")]
        [TestCase("https://github.com/github/VisualStudio/blob/master/README.md", "github.com")]
        public void Host(string url, string expectHost)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromUrl(url);

            Assert.That(context.Host, Is.EqualTo(expectHost));
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
            var target = new GitHubContextService();

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
            var target = new GitHubContextService();

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
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context?.Branch, Is.EqualTo(expectBranch));
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
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Owner, Is.EqualTo(expectOwner));
            Assert.That(context.RepositoryName, Is.EqualTo(expectRepositoryName));
            Assert.That(context.Branch, Is.EqualTo(expectBranch));
        }

        [TestCase("[spike] Open from GitHub URL by jcansdale · Pull Request #1763 · github/VisualStudio - Google Chrome", 1763)]
        public void PullRequest(string windowTitle, int expectPullRequest)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.PullRequest, Is.EqualTo(expectPullRequest));
        }

        [TestCase("Consider adding C# code style preferences to editorconfig · Issue #1750 · github/VisualStudio - Google Chrome", 1750)]
        public void Issue(string windowTitle, int expectIssue)
        {
            var target = new GitHubContextService();

            var context = target.FindContextFromWindowTitle(windowTitle);

            Assert.That(context.Issue, Is.EqualTo(expectIssue));
        }
    }
}
