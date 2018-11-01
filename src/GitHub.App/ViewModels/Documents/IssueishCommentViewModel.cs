using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model for comments on an issue or pull request.
    /// </summary>
    [Export(typeof(IIssueishCommentViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IssueishCommentViewModel : CommentViewModel, IIssueishCommentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="commentService">The comment service.</param>
        [ImportingConstructor]
        public IssueishCommentViewModel(ICommentService commentService)
            : base(commentService)
        {
        }

        /// <inheritdoc/>
        public bool CanCloseIssueish { get; private set; }

        /// <inheritdoc/>
        public string CloseIssueishCaption { get; private set; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> CloseIssueish { get; }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            ICommentThreadViewModel thread,
            ActorModel currentUser,
            CommentModel comment,
            string closeCaption)
        {
            await base.InitializeAsync(
                thread,
                currentUser,
                comment,
                comment == null ? CommentEditState.Editing : CommentEditState.None)
                    .ConfigureAwait(true);

            CanCloseIssueish = closeCaption != null;
            CloseIssueishCaption = closeCaption;
        }
    }
}
