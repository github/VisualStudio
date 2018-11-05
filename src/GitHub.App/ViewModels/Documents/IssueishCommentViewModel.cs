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
        ObservableAsPropertyHelper<string> closeOrReopenCaption;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="commentService">The comment service.</param>
        [ImportingConstructor]
        public IssueishCommentViewModel(ICommentService commentService)
            : base(commentService)
        {
            CloseOrReopen = ReactiveCommand.CreateFromTask(
                DoCloseOrReopen,
                this.WhenAnyValue(x => x.CanCloseOrReopen));
        }

        /// <inheritdoc/>
        public bool CanCloseOrReopen { get; private set; }

        /// <inheritdoc/>
        public string CloseOrReopenCaption => closeOrReopenCaption?.Value;

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> CloseOrReopen { get; }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            IIssueishCommentThreadViewModel thread,
            ActorModel currentUser,
            CommentModel comment,
            bool isPullRequest,
            bool isOpen,
            bool canCloseOrReopen)
        {
            await base.InitializeAsync(
                thread,
                currentUser,
                comment,
                comment == null ? CommentEditState.Editing : CommentEditState.None)
                    .ConfigureAwait(true);

            CanCloseOrReopen = canCloseOrReopen;
            closeOrReopenCaption?.Dispose();

            if (canCloseOrReopen)
            {
                var caption = isPullRequest ?
                    isOpen ?
                        (Resources.ClosePullRequest, Resources.CloseAndComment) :
                        (Resources.ReopenPullRequest, Resources.ReopenAndComment) :
                    isOpen ?
                        (Resources.CloseIssue, Resources.CloseAndComment) :
                        (Resources.ReopenIssue, Resources.ReopenAndComment);

                closeOrReopenCaption = this.WhenAnyValue(x => x.Body)
                    .Select(x => string.IsNullOrWhiteSpace(x) ? caption.Item1 : caption.Item2)
                    .ToProperty(this, x => x.CloseOrReopenCaption);
            }
        }

        Task DoCloseOrReopen()
        {
            return Task.CompletedTask;
        }
    }
}
