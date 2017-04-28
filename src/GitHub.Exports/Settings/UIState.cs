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
        PullRequestDetailUIState prState = new PullRequestDetailUIState();

        /// <summary>
        /// Gets or sets the UI state for the <see cref="IGitHubConnectSection"/>s in Team Explorer.
        /// </summary>
        public List<GitHubConnectSectionState> GitHubConnectSections { get; set; }
            = new List<GitHubConnectSectionState>();

        /// <summary>
        /// Gets or sets a a collection of UI state objects for repositories.
        /// </summary>
        public List<RepositoryUIState> RepositoryState { get; set; }
            = new List<RepositoryUIState>();

        /// <summary>
        /// Gets or sets global settings for the Pull Request detail view.
        /// </summary>
        public PullRequestDetailUIState PullRequestDetailState
        {
            get { return prState; }
            set
            {
                if (value != null)
                {
                    prState = value;
                }
            }
        }

        /// <summary>
        /// Gets or creates the UI state for a repository.
        /// </summary>
        /// <param name="repositoryUrl">The URL of the repository.</param>
        /// <returns>A <see cref="RepositoryUIState"/> object.</returns>
        public RepositoryUIState GetOrCreateRepositoryState(string repositoryUrl)
        {
            var result = RepositoryState.FirstOrDefault(x => x.RepositoryUrl == repositoryUrl);

            if (result == null)
            {
                result = new RepositoryUIState { RepositoryUrl = repositoryUrl };
                RepositoryState.Add(result);
            }

            return result;
        }

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
