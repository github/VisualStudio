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

        /// <summary>
        /// Displays a message box with the specified message.
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <returns>The result.</returns>
        VSConstants.MessageBoxResult ShowMessageBoxInfo(string message);
    }
}