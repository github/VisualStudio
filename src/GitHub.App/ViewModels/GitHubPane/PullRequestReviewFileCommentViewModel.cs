using System;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// A view model for a file comment in a <see cref="PullRequestReviewViewModel"/>.
    /// </summary>
    public class PullRequestReviewFileCommentViewModel : IPullRequestReviewFileCommentViewModel
    {
        public PullRequestReviewFileCommentViewModel(IPullRequestReviewCommentModel model)
        {
            Body = model.Body;
            RelativePath = model.Path;
        }

        /// <inheritdoc/>
        public string Body { get; }

        /// <inheritdoc/>
        public string RelativePath { get; }
    }
}
