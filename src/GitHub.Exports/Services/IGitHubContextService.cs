using System;
using System.Collections.Generic;

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
    }
}