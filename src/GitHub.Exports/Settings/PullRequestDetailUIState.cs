using System;

namespace GitHub.Settings
{
    /// <summary>
    /// Holds global settings for the Pull Request detail view.
    /// </summary>
    public class PullRequestDetailUIState
    {
        /// <summary>
        /// Gets or sets a value indicating whether a "diff" or "open" should be carried out when
        /// double clicking a changed file.
        /// </summary>
        /// <remarks>
        /// Ideally this would be an enum but SimpleJson doesn't handle enums.
        /// </remarks>
        public bool DiffOnOpen { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether changed files should be displayed as a tree or
        /// a list.
        /// </summary>
        /// <remarks>
        /// Ideally this would be an enum but SimpleJson doesn't handle enums.
        /// </remarks>
        public bool ShowTree { get; set; } = true;
    }
}
