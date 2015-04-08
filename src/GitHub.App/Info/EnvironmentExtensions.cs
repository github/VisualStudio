using System.IO;
using System;

namespace GitHub.Info
{
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
