using System;
using System.Collections.Generic;
using LibGit2Sharp;

namespace GitHub.Services
{
    public interface IGitHubContextService
    {
        GitHubContext FindContextFromClipboard();
        GitHubContext FindContextFromBrowser();
        GitHubContext FindContextFromUrl(string url);
        GitHubContext FindContextFromWindowTitle(string windowTitle);
        IEnumerable<string> FindWindowTitlesForClass(string className = "MozillaWindowClass");
        string ResolvePath(GitHubContext context);
        Uri ToRepositoryUrl(GitHubContext context);
        bool TryOpenFile(string repositoryDir, GitHubContext context);
        (string commitish, string path) ResolveGitObject(string repositoryDir, GitHubContext context);
        bool HasChangesInWorkingDirectory(string repositoryDir, string commitish, string path);
    }
}