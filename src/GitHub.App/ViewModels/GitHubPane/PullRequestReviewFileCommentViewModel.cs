using System;
using System.Reactive;
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
        }

        /// <inheritdoc/>
        public string Body => model.Body;

        /// <inheritdoc/>
        public string RelativePath => model.Path;

        /// <inheritdoc/>
        public ReactiveCommand<Unit> Open { get; }
    }
}