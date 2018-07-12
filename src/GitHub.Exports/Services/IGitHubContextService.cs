using System;
using System.Threading.Tasks;

namespace GitHub.Services
{
    public interface IGitHubContextService
    {
        GitHubContext FindContextFromClipboard();
        GitHubContext FindContextFromBrowser();
        GitHubContext FindContextFromUrl(string url);
        GitHubContext FindContextFromWindowTitle(string windowTitle);
        Uri ToRepositoryUrl(GitHubContext context);
        bool TryOpenFile(string repositoryDir, GitHubContext context);
        Task<bool> TryAnnotateFile(string repositoryDir, string currentBranch, GitHubContext context);
        (string commitish, string path, string commitSha) ResolveBlob(string repositoryDir, GitHubContext context, string remoteName = "origin");
        bool HasChangesInWorkingDirectory(string repositoryDir, string commitish, string path);
    }
}