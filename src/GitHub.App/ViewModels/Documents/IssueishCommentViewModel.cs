using System;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
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
        ObservableAsPropertyHelper<string> closeIssueishCaption;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="commentService">The comment service.</param>
        [ImportingConstructor]
        public IssueishCommentViewModel(ICommentService commentService)
            : base(commentService)
        {
            CloseIssueish = ReactiveCommand.CreateFromTask(
                DoCloseIssueish,
                this.WhenAnyValue(x => x.CanCloseIssueish));
        }

        /// <inheritdoc/>
        public bool CanCloseIssueish { get; private set; }

        /// <inheritdoc/>
        public string CloseIssueishCaption => closeIssueishCaption?.Value;

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> CloseIssueish { get; }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            IIssueishCommentThreadViewModel thread,
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

            if (closeCaption != null)
            {
                closeIssueishCaption = this.WhenAnyValue(x => x.Body)
                    .Select(x => string.IsNullOrWhiteSpace(x) ? closeCaption : Resources.CloseAndComment)
                    .ToProperty(this, x => x.CloseIssueishCaption);
            }
        }

        Task DoCloseIssueish()
        {
            return Task.CompletedTask;
        }
    }
}
