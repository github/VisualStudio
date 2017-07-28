using System;
using Octokit;

namespace GitHub.Extensions
{
    public static class ApiExceptionExtensions
    {
        const string GithubHeader = "X-GitHub-Request-Id";
        public static bool IsGitHubApiException(this Exception ex)
        {
            var apiex = ex as ApiException;
            return apiex?.HttpResponse?.Headers.ContainsKey(GithubHeader) ?? false;
        }
    }

}
