using GitHub.ViewModels.GitHubPane;

namespace GitHub.Models.Drafts
{
    /// <summary>
    /// Stores a draft for a <see cref="PullRequestCreationViewModel"/>.
    /// </summary>
    public class PullRequestDraft
    {
        /// <summary>
        /// Gets or sets the draft pull request title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the draft pull request body.
        /// </summary>
        public string Body { get; set; }
    }
}
