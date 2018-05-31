using System;
using System.Collections.Generic;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// View model for displaying details of a pull request review.
    /// </summary>
    public class PullRequestReviewViewModel : ViewModelBase, IPullRequestReviewViewModel
    {
        bool isExpanded;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestReviewViewModel"/> class.
        /// </summary>
        /// <param name="editorService">The pull request editor service.</param>
        /// <param name="session">The pull request session.</param>
        /// <param name="model">The pull request review model.</param>
        public PullRequestReviewViewModel(
            IPullRequestEditorService editorService,
            IPullRequestSession session,
            PullRequestReviewModel model)
        {
            Guard.ArgumentNotNull(editorService, nameof(editorService));
            Guard.ArgumentNotNull(session, nameof(session));
            Guard.ArgumentNotNull(model, nameof(model));

            Model = model;
            Body = string.IsNullOrWhiteSpace(Model.Body) ? null : Model.Body;
            StateDisplay = ToString(Model.State);

            var comments = new List<IPullRequestReviewFileCommentViewModel>();
            var outdated = new List<IPullRequestReviewFileCommentViewModel>();

            foreach (var comment in model.Comments)
            {
                if (comment.Thread != null)
                {
                    var vm = new PullRequestReviewCommentViewModel(
                        editorService,
                        session,
                        comment.Thread.Path,
                        comment);

                    if (comment.Thread.Position != null)
                        comments.Add(vm);
                    else
                        outdated.Add(vm);
                }
            }

            FileComments = comments;
            OutdatedFileComments = outdated;

            HasDetails = Body != null ||
                FileComments.Count > 0 ||
                OutdatedFileComments.Count > 0;
        }

        /// <inheritdoc/>
        public PullRequestReviewModel Model { get; }

        /// <inheritdoc/>
        public string Body { get; }

        /// <inheritdoc/>
        public string StateDisplay { get; }

        /// <inheritdoc/>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { this.RaiseAndSetIfChanged(ref isExpanded, value); }
        }

        /// <inheritdoc/>
        public bool HasDetails { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestReviewFileCommentViewModel> FileComments { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestReviewFileCommentViewModel> OutdatedFileComments { get; }

        static string ToString(PullRequestReviewState state)
        {
            switch (state)
            {
                case PullRequestReviewState.Approved:
                    return "approved";
                case PullRequestReviewState.ChangesRequested:
                    return "requested changes";
                case PullRequestReviewState.Commented:
                case PullRequestReviewState.Dismissed:
                    return "commented";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}