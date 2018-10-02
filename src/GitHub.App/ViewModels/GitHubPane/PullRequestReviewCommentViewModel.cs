using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model for a file comment in a <see cref="PullRequestReviewViewModel"/>.
    /// </summary>
    public class PullRequestReviewCommentViewModel : IPullRequestReviewFileCommentViewModel
    {
        readonly IPullRequestEditorService editorService;
        readonly IPullRequestSession session;
        readonly PullRequestReviewCommentModel model;
        IInlineCommentThreadModel thread;

        public PullRequestReviewCommentViewModel(
            IPullRequestEditorService editorService,
            IPullRequestSession session,
            string relativePath,
            PullRequestReviewCommentModel model)
        {
            Guard.ArgumentNotNull(editorService, nameof(editorService));
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(model, nameof(model));

            this.editorService = editorService;
            this.session = session;
            this.model = model;
            RelativePath = relativePath;

            Open = ReactiveCommand.CreateFromTask(DoOpen);
        }

        /// <inheritdoc/>
        public string Body => model.Body;

        /// <inheritdoc/>
        public string RelativePath { get; set; }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> Open { get; }

        async Task DoOpen()
        {
            try
            {
                if (thread == null)
                {
                    var file = await session.GetFile(RelativePath, model.Thread.CommitSha);
                    thread = file.InlineCommentThreads.FirstOrDefault(t => t.Comments.Any(c => c.Comment.Id == model.Id));
                }

                if (thread != null && thread.LineNumber != -1)
                {
                    await editorService.OpenDiff(session, RelativePath, thread);
                }
            }
            catch (Exception)
            {
                // TODO: Show error.
            }
        }
    }
}