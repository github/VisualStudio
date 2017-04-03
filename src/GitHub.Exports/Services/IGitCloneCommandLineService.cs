using GitHub.Primitives;

namespace GitHub.Services
{
    public interface IGitCloneCommandLineService
    {
        /// <summary>
        /// Find when a GitHub URL has been passed into Visual Studio using the '/GitClone' option.
        /// </summary>
        /// <example>
        /// This is used by the 'git-client' URL protocol hendler.
        /// git-client://clone?repo=https%3A%2F%2Fgithub.com%2Fgithub%2FVisualStudio
        /// will open Visual Studio with
        /// /GitClone https://github.com/github/VisualStudio
        /// </example>
        /// <returns>A GitHub URI or null.</returns>
        UriString FindGitHubCloneOption();

        /// <summary>
        /// Open a repository if one exists relative to LocalClonePath or is in list of KnownRepositories.
        /// </summary>
        /// <param name="cloneUri"></param>
        /// <returns>True if repository was opened.</returns>
        bool TryOpenRepository(UriString cloneUri);
    }
}