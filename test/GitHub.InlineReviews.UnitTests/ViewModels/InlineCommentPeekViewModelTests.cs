using System;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.InlineReviews.Services;
using GitHub.InlineReviews.ViewModels;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using NSubstitute;
using ReactiveUI;
using Xunit;

namespace GitHub.InlineReviews.UnitTests.ViewModels
{
    public class InlineCommentPeekViewModelTests
    {
        const string FullPath = "c:\\repo\\test.cs";
        const string RelativePath = "test.cs";

        [Fact]
        public async Task ThreadIsCreatedForExistingComments()
        {
            // There is an existing comment thread at line 10.
            var target = new InlineCommentPeekViewModel(
                Substitute.For<IApiClientFactory>(),
                CreatePeekService(lineNumber: 10),
                CreatePeekSession(),
                CreateSessionManager());

            await target.Initialize();

            // There should be an existing comment and a reply placeholder.
            Assert.IsType<InlineCommentThreadViewModel>(target.Thread);
            Assert.Equal(2, target.Thread.Comments.Count);
            Assert.Equal("Existing comment", target.Thread.Comments[0].Body);
            Assert.Equal(string.Empty, target.Thread.Comments[1].Body);
            Assert.Equal(CommentEditState.Placeholder, target.Thread.Comments[1].EditState);
        }

        [Fact]
        public async Task ThreadIsCreatedForNewComment()
        {
            // There is no existing comment thread at line 9, but there is a + diff entry.
            var target = new InlineCommentPeekViewModel(
                Substitute.For<IApiClientFactory>(),
                CreatePeekService(lineNumber: 9),
                CreatePeekSession(),
                CreateSessionManager());

            await target.Initialize();

            Assert.IsType<NewInlineCommentThreadViewModel>(target.Thread);
            Assert.Equal(string.Empty, target.Thread.Comments[0].Body);
            Assert.Equal(CommentEditState.Editing, target.Thread.Comments[0].EditState);
        }

        IInlineCommentPeekService CreatePeekService(int lineNumber)
        {
            var result = Substitute.For<IInlineCommentPeekService>();
            result.GetLineNumber(Arg.Any<IPeekSession>()).Returns(lineNumber);
            return result;
        }

        IPeekSession CreatePeekSession()
        {
            var document = Substitute.For<ITextDocument>();
            document.FilePath.Returns(FullPath);

            var propertyCollection = new PropertyCollection();
            propertyCollection.AddProperty(typeof(ITextDocument), document);

            var result = Substitute.For<IPeekSession>();
            result.TextView.TextBuffer.Properties.Returns(propertyCollection);

            return result;
        }

        IPullRequestSessionManager CreateSessionManager(string commitSha = "COMMIT")
        {
            var comment = Substitute.For<IPullRequestReviewCommentModel>();
            comment.Body.Returns("Existing comment");
            comment.OriginalPosition.Returns(10);

            var thread = Substitute.For<IInlineCommentThreadModel>();
            thread.Comments.Returns(new ReactiveList<IPullRequestReviewCommentModel>(new[] { comment }));
            thread.LineNumber.Returns(10);

            var diff = new DiffChunk
            {
                DiffLine = 10,
                OldLineNumber = 1,
                NewLineNumber = 1,
            };

            for (var i = 0; i < 10; ++i)
            {
                diff.Lines.Add(new DiffLine
                {
                    NewLineNumber = i,
                    DiffLineNumber = i + 10,
                    Type = i < 5 ? DiffChangeType.Delete : DiffChangeType.Add,
                });
            }

            var file = Substitute.For<IPullRequestSessionFile>();
            file.CommitSha.Returns(commitSha);
            file.Diff.Returns(new[] { diff });
            file.InlineCommentThreads.Returns(new[] { thread });

            var session = Substitute.For<IPullRequestSession>();
            session.GetFile(RelativePath).Returns(file);
            session.GetRelativePath(FullPath).Returns(RelativePath);
            session.Repository.CloneUrl.Returns(new UriString("https://foo.bar"));

            var result = Substitute.For<IPullRequestSessionManager>();
            result.CurrentSession.Returns(session);

            return result;
        }
    }
}
