using Microsoft.VisualStudio;

namespace GitHub.Services
{
    public interface IVSServices
    {
        /// <summary>
        /// Get the full Visual Studio version from `VisualStudio\14.0_Config\SplashInfo|EnvVersion` on Visual Studoi 2015
        /// or `SetupConfiguration.GetInstanceForCurrentProcess()` on on Visual Studoi 2017.
        /// </summary>
        string VSVersion { get; }

        /// <summary>Open a repository in Team Explorer</summary>
        /// <remarks>
        /// There doesn't appear to be a command that directly opens a target repo.
        /// Our workaround is to create, open and delete a solution in the repo directory.
        /// This triggers an event that causes the target repo to open. ;)
        /// </remarks>
        /// <param name="directory">The path to the repository to open</param>
        /// <returns>True if a transient solution was successfully created in target directory (which should trigger opening of repository).</returns>
        bool TryOpenRepository(string directory);

        /// <summary>
        /// Displays a message box with the specified message.
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <returns>The result.</returns>
        VSConstants.MessageBoxResult ShowMessageBoxInfo(string message);
    }
}