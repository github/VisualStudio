using System.IO;
using Rothko;

namespace GitHub.Extensions
{
    public static class EnvironmentExtensions
    {
#if DEBUG
        const string CacheName = "GìtHūbForVisualStudio";
#else
        const string CacheName = "GitHubForVisualStudio";
#endif

        public static string GetLocalGitHubApplicationDataPath(this IEnvironment environment)
        {
            return Path.Combine(environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                CacheName);
        }
    }
}
