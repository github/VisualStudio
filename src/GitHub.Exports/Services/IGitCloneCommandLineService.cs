namespace GitHub.Services
{
    public interface IGitCloneCommandLineService
    {
        /// <summary>
        /// Find the '/GitClone' option which might be passed into Visual Studio.
        /// </summary>
        /// <example>
        /// This is used by the 'git-client' URL protocol hendler.
        /// git-client://clone?repo=https%3A%2F%2Fgithub.com%2Fgithub%2FVisualStudio
        /// will open Visual Studio with
        /// /GitClone https://github.com/github/VisualStudio
        /// </example>
        /// <returns>The /GitClone option or null.</returns>
        string FindGitCloneOption();

        /// <summary>
        /// Open a repository if one exists relative to LocalClonePath or is in list of KnownRepositories.
        /// </summary>
        /// <param name="cloneUrl"></param>
        /// <returns>True if repository was opened.</returns>
        bool TryOpenRepository(string cloneUrl);
    }
}