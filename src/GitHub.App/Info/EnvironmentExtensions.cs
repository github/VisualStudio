using System.IO;
using System;

namespace GitHub.Info
{
    public static class EnvironmentExtensions
    {
        const string applicationName = "GitHub";

        public static string GetLocalGitHubApplicationDataPath(this Rothko.IEnvironment environment)
        {
            return Path.Combine(environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                applicationName);
        }

        public static string GetApplicationDataPath(this Rothko.IEnvironment environment)
        {
            return Path.Combine(environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName);
        }

        public static string GetProgramFilesPath(this Rothko.IEnvironment environment)
        {
            return environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }
    }
}
