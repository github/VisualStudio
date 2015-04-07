using System.IO;
using System;

namespace GitHub.Info
{
    public static class ApplicationInfo
    {
#if DEBUG
        public const string ApplicationName = "GìtHūbForVisualStudio";
        public const string ApplicationProvider = "GìtHūb";
#else
        public const string ApplicationName = "GitHubForVisualStudio";
        public const string ApplicationProvider = "GitHub";
#endif
        public const string ApplicationDescription = "GitHub Extension for Visual Studio";
    }

    public static class EnvironmentExtensions
    {
        public static string GetLocalGitHubApplicationDataPath(this Rothko.IEnvironment environment)
        {
            return Path.Combine(environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationInfo.ApplicationName);
        }

        public static string GetApplicationDataPath(this Rothko.IEnvironment environment)
        {
            return Path.Combine(environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationInfo.ApplicationName);
        }

        public static string GetProgramFilesPath(this Rothko.IEnvironment environment)
        {
            return environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        public static string GetUserDocumentsPathForApplication(this Rothko.IEnvironment environment)
        {
            return Path.Combine(environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ApplicationInfo.ApplicationName);
        }
    }
}
