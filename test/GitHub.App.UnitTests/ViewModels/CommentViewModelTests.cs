using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace GitHub.App.UnitTests.ViewModels
{
    public class CommentViewModelTests
    {
        [Test]
        public async Task CommitCaption_Is_Comment_When_Id_Null()
        {
            var target = await CreateAndInitializeTarget(new CommentModel());

            Assert.That(target.CommitCaption, Is.EqualTo("Comment"));
        }

        [Test]
        public async Task CommitCaption_Is_Update_When_Id_Not_Null()
        {
            var target = await CreateAndInitializeTarget(new CommentModel { Id = "existing" });

            Assert.That(target.CommitCaption, Is.EqualTo("Update comment"));
        }

        [Test]
        public async Task CanCancel_Is_False_When_Id_Null()
        {
            var target = await CreateAndInitializeTarget(new CommentModel());

            Assert.False(target.CanCancel);
        }

        [Test]
        public async Task CanCancel_Is_True_When_Id_Not_Null()
        {
            var target = await CreateAndInitializeTarget(new CommentModel { Id = "existing" });

            Assert.True(target.CanCancel);
        }

        async Task<CommentViewModel> CreateAndInitializeTarget(
            CommentModel comment,
            ICommentService commentService = null,
            ICommentThreadViewModel thread = null,
            ActorModel currentUser = null,
            CommentEditState state = CommentEditState.Editing)
        {
            thread = thread ?? Substitute.For<ICommentThreadViewModel>();
            currentUser = currentUser ?? new ActorModel { Login = "grokys" };

            var target = CreateTarget(commentService);
            await target.InitializeAsync(
                thread,
                currentUser,
                comment,
                state);
            return target;
        }

        CommentViewModel CreateTarget(
            ICommentService commentService = null)
        {
            commentService = commentService ?? Substitute.For<ICommentService>();

            return new CommentViewModel(commentService);
        }
    }
}
