using System;
using GitHub.Settings;

namespace GitHub.Settings
{
    /// <summary>
    /// Stores persistent UI state for a <see cref="GitHubConnectSection"/> in 
    /// <see cref="IPackageSettings"/>.
    /// </summary>
    public class GitHubConnectSectionState
    {
        /// <summary>
        /// Gets or sets the name of the connection section.
        /// </summary>
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the section should be expanded.
        /// </summary>
        public bool IsExpanded { get; set; } = true;
    }
}
