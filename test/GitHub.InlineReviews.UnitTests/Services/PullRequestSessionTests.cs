using System;
using System.Linq;
using System.Threading.Tasks;
using GitHub.InlineReviews.Services;
using GitHub.Models;
using GitHub.Services;
using LibGit2Sharp;
using NSubstitute;
using Rothko;
using Xunit;

namespace GitHub.InlineReviews.UnitTests.Services
{
    public class PullRequestSessionTests
    {
        const string RepoUrl = "https://foo.bar/owner/repo";

        public class TheGetFileMethod
        {
            [Fact]
            public async Task ReviewCommentIsCreated()
            {
                const string path = "test.cs";
                var comment = Substitute.For<IPullRequestReviewCommentModel>();
                comment.DiffHunk.Returns(@"@@ -11,21 +11,21 @@
 using GitHub.Extensions;
 using System.Threading.Tasks;
 using GitHub.Helpers;
+using System.Threading;");
                comment.Path.Returns(path);
                comment.OriginalCommitId.Returns("ORIG");
                comment.OriginalPosition.Returns(3);

                var changedFile = Substitute.For<IPullRequestFileModel>();
                changedFile.FileName.Returns("test.cs");

                var pullRequest = Substitute.For<IPullRequestModel>();
                pullRequest.Base.Returns(new GitReferenceModel("BASE", "master", "BASE", RepoUrl));
                pullRequest.Head.Returns(new GitReferenceModel("HEAD", "pr", "HEAD", RepoUrl));
                pullRequest.ChangedFiles.Returns(new[] { changedFile });
                pullRequest.ReviewComments.Returns(new[] { comment });

                var repository = Substitute.For<IRepository>();
                var branch = Substitute.For<Branch>();
                var commit = Substitute.For<Commit>();
                branch.Tip.Returns(commit);
                repository.Head.Returns(branch);

                var gitService = Substitute.For<IGitService>();
                gitService.GetRepository(Arg.Any<string>()).Returns(repository);

                var changes = Substitute.For<ContentChanges>();
                changes.Patch.Returns(Properties.Resources.pr_960_diff);

                var gitClient = Substitute.For<IGitClient>();
                gitClient.IsModified(repository, "test.cs", Arg.Any<byte[]>()).Returns(false);
                gitClient.CompareWith(repository, "BASE", path, Arg.Any<byte[]>()).Returns(changes);

                var os = Substitute.For<IOperatingSystem>();

                var target = new PullRequestSession(
                    os,
                    gitService,
                    gitClient,
                    new DiffService(),
                    Substitute.For<IAccount>(),
                    pullRequest,
                    Substitute.For<ILocalRepositoryModel>(),
                    true);

                var file = await target.GetFile(path);
                var thread = file.InlineCommentThreads.First();

                Assert.Equal(13, thread.LineNumber);
            }
        }
    }
}
