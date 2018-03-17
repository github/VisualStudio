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
    public class PullRequestReviewFileCommentViewModel : IPullRequestReviewFileCommentViewModel
    {
        readonly IPullRequestEditorService editorService;
        readonly IPullRequestSession session;
        readonly IPullRequestReviewCommentModel model;
        IInlineCommentThreadModel thread;

        public PullRequestReviewFileCommentViewModel(
            IPullRequestEditorService editorService,
            IPullRequestSession session,
            IPullRequestReviewCommentModel model)
        {
            Guard.ArgumentNotNull(editorService, nameof(editorService));
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(model, nameof(model));

            this.editorService = editorService;
            this.session = session;
            this.model = model;
            Open = ReactiveCommand.CreateAsyncTask(DoOpen);
        }

        /// <inheritdoc/>
        public string Body => model.Body;

        /// <inheritdoc/>
        public string RelativePath => model.Path;

        /// <inheritdoc/>
        public ReactiveCommand<Unit> Open { get; }

        async Task DoOpen(object o)
        {
            try
            {
                if (thread == null)
                {
                    var commit = model.Position.HasValue ? model.CommitId : model.OriginalCommitId;
                    var file = await session.GetFile(RelativePath, commit);
                    thread = file.InlineCommentThreads.FirstOrDefault(t => t.Comments.Any(c => c.Id == model.Id));
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
