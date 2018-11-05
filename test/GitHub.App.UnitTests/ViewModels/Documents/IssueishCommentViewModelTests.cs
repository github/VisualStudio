using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels.Documents;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.App.UnitTests.ViewModels.Documents
{
    public class IssueishCommentViewModelTests
    {
        [Test]
        public async Task CloseOrReopenCaption_Is_Set_When_Body_Empty()
        {
            var target = await CreateAndInitializeTarget(
                new CommentModel(),
                canCloseOrReopen: true);

            Assert.That(target.CloseOrReopenCaption, Is.EqualTo("Close issue"));
        }

        [Test]
        public async Task CloseOrReopenCaption_Is_Updated_When_Body_Not_Empty()
        {
            var target = await CreateAndInitializeTarget(
                new CommentModel(),
                canCloseOrReopen: true);

            target.Body = "Body";

            Assert.That(target.CloseOrReopenCaption, Is.EqualTo("Close and comment"));
        }

        async Task<IssueishCommentViewModel> CreateAndInitializeTarget(
            CommentModel comment,
            bool canCloseOrReopen = false,
            ICommentService commentService = null,
            IIssueishCommentThreadViewModel thread = null,
            ActorModel currentUser = null)
        {
            thread = thread ?? Substitute.For<IIssueishCommentThreadViewModel>();
            currentUser = currentUser ?? new ActorModel { Login = "grokys" };

            var target = CreateTarget(commentService);
            await target.InitializeAsync(
                thread,
                currentUser,
                comment,
                false,
                true,
                canCloseOrReopen);
            return target;
        }

        IssueishCommentViewModel CreateTarget(
            ICommentService commentService = null)
        {
            commentService = commentService ?? Substitute.For<ICommentService>();

            return new IssueishCommentViewModel(commentService);
        }
    }
}
