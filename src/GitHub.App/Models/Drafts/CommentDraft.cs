using GitHub.ViewModels;

namespace GitHub.Models.Drafts
{
    /// <summary>
    /// Stores a draft for a <see cref="CommentViewModel"/>
    /// </summary>
    public class CommentDraft
    {
        /// <summary>
        /// Gets or sets the draft comment body.
        /// </summary>
        public string Body { get; set; }
    }
}
