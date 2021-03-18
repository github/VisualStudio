using System;
using System.Linq;
using Octokit;

namespace GitHub.Extensions
{
    public static class ApiExceptionExtensions
    {
        const string GithubHeader = "X-GitHub-Request-Id";
        public static bool IsGitHubApiException(this Exception ex)
        {
            var apiex = ex as ApiException;
            return apiex?.HttpResponse?.Headers.Keys.Contains(GithubHeader, StringComparer.OrdinalIgnoreCase) ?? false;
        }
    }
}
