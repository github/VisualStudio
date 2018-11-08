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
    public sealed class IssueishCommentViewModel : CommentViewModel, IIssueishCommentViewModel
    {
        bool canCloseOrReopen;
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
            AddErrorHandler(CloseOrReopen);
        }

        /// <inheritdoc/>
        public bool CanCloseOrReopen
        {
            get => canCloseOrReopen;
            private set => this.RaiseAndSetIfChanged(ref canCloseOrReopen, value);
        }

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
            bool canCloseOrReopen,
            IObservable<bool> isOpen = null)
        {
            await base.InitializeAsync(
                thread,
                currentUser,
                comment,
                comment == null ? CommentEditState.Editing : CommentEditState.None)
                    .ConfigureAwait(true);

            CanCloseOrReopen = canCloseOrReopen;
            closeOrReopenCaption?.Dispose();

            if (canCloseOrReopen && isOpen != null)
            {
                closeOrReopenCaption =
                    this.WhenAnyValue(x => x.Body)
                    .CombineLatest(isOpen, (body, open) => GetCloseOrReopenCaption(isPullRequest, open, body))
                    .ToProperty(this, x => x.CloseOrReopenCaption);
            }
        }

        public void Dispose() => closeOrReopenCaption.Dispose();

        async Task DoCloseOrReopen()
        {
            await ((IIssueishCommentThreadViewModel)Thread).CloseOrReopen(this).ConfigureAwait(true);
        }

        static string GetCloseOrReopenCaption(bool isPullRequest, bool isOpen, string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                if (isPullRequest)
                {
                    return isOpen ? Resources.ClosePullRequest : Resources.ReopenPullRequest;
                }
                else
                {
                    return isOpen ? Resources.CloseIssue: Resources.ReopenIssue;
                }
            }
            else
            {
                return isOpen ? Resources.CloseAndComment : Resources.ReopenAndComment;
            }
        }
    }
}
