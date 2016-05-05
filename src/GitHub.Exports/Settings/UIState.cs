using System;
using System.Collections.Generic;
using GitHub.Settings;
using System.Linq;
using GitHub.VisualStudio.TeamExplorer.Connect;

namespace GitHub.Settings
{
    /// <summary>
    /// Stores persistent UI state in <see cref="IPackageSettings"/>.
    /// </summary>
    public class UIState
    {
        /// <summary>
        /// Gets or sets the UI state for the <see cref="IGitHubConnectSection"/>s in Team Explorer.
        /// </summary>
        public List<GitHubConnectSectionState> GitHubConnectSections { get; set; }
            = new List<GitHubConnectSectionState>();

        /// <summary>
        /// Gets or creates the UI state for a named <see cref="IGitHubConnectSection"/>.
        /// </summary>
        /// <param name="sectionName">The name of the section.</param>
        /// <returns>A <see cref="GitHubConnectSectionState"/> object.</returns>
        public GitHubConnectSectionState GetOrCreateConnectSection(string sectionName)
        {
            var result = GitHubConnectSections.FirstOrDefault(x => x.SectionName == sectionName);

            if (result == null)
            {
                result = new GitHubConnectSectionState { SectionName = sectionName };
                GitHubConnectSections.Add(result);
            }

            return result;
        }
    }
}
