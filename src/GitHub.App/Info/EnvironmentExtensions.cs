using System.IO;
using Rothko;
using Environment = System.Environment;

namespace GitHub
{
    public static class EnvironmentExtensions
    {
        const string applicationName = "GitHub";

        public static string GetLocalGitHubApplicationDataPath(this IEnvironment environment)
        {
            return Path.Combine(environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                applicationName);
        }

        public static string GetApplicationDataPath(this IEnvironment environment)
        {
            return Path.Combine(environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName);
        }

        public static string GetProgramFilesPath(this IEnvironment environment)
        {
            return environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }
    }
}
