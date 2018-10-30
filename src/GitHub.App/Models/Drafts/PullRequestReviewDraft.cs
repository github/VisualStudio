using GitHub.ViewModels.GitHubPane;

namespace GitHub.Models.Drafts
{
    /// <summary>
    /// Stores a draft for a <see cref="PullRequestReviewAuthoringViewModel"/>.
    /// </summary>
    public class PullRequestReviewDraft
    {
        /// <summary>
        /// Gets or sets the draft review body.
        /// </summary>
        public string Body { get; set; }
    }
}
