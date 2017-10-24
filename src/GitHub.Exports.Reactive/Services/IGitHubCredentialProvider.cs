using LibGit2Sharp;

namespace GitHub.Services
{
    public interface IGitHubCredentialProvider
    {
        Credentials HandleCredentials(string url, string username, SupportedCredentialTypes types);
    }
}