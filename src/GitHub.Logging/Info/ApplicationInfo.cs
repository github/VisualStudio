using System;
using System.Diagnostics;

namespace GitHub.Info
{
    public static class ApplicationInfo
    {
#if DEBUG
        public const string ApplicationName = "GìtHūbVisualStudio";
        public const string ApplicationProvider = "GitHub";
#else
        public const string ApplicationName = "GitHubVisualStudio";
        public const string ApplicationProvider = "GitHub";
#endif
        public const string ApplicationSafeName = "GitHubVisualStudio";
        public const string ApplicationDescription = "GitHub Extension for Visual Studio";

        /// <summary>
        /// Gets the version information for the host process.
        /// </summary>
        /// <returns>The version of the host process.</returns>
        public static FileVersionInfo GetHostVersionInfo()
        {
            return Process.GetCurrentProcess().MainModule.FileVersionInfo;
        }

        /// <summary>
        /// Gets the version of a Visual Studio package.
        /// </summary>
        /// <param name="package">
        /// The VS Package object. This is untyped here as this assembly does not depend on
        /// any VS assemblies.
        /// </param>
        /// <returns>The version of the package.</returns>
        public static Version GetPackageVersion(object package)
        {
            return package.GetType().Assembly.GetName().Version;
        }
    }
}
