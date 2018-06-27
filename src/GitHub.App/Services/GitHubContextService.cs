using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using GitHub.Primitives;

namespace GitHub.App.Services
{
    [Export(typeof(GitHubContextService))]
    public class GitHubContextService
    {
        // USERID_REGEX = /[a-z0-9][a-z0-9\-\_]*/i
        const string owner = "(?<owner>[a-zA-Z0-9][a-zA-Z0-9-_]*)";

        // REPO_REGEX = /(?:\w|\.|\-)+/i
        // This supports "_" for legacy superfans with logins that still contain "_".
        const string repo = @"(?<repo>(?:\w|\.|\-)+)";

        //BRANCH_REGEX = /[^\/]+(\/[^\/]+)?/
        const string branch = @"(?<branch>[^./ ~^:?*\[\\][^/ ~^:?*\[\\]*(/[^./ ~^:?*\[\\][^/ ~^:?*\[\\]*)*)";

        const string pull = "(?<pull>[0-9]+)";

        const string issue = "(?<issue>[0-9]+)";

        static readonly string path = $"^{repo}/(?<path>[^ ]+)";

        static readonly Regex windowTitleRepositoryRegex = new Regex($"^{owner}/{repo}: ", RegexOptions.Compiled);
        static readonly Regex windowTitleBranchRegex = new Regex($"^{owner}/{repo} at {branch} ", RegexOptions.Compiled);
        static readonly Regex windowTitlePullRequestRegex = new Regex($" · Pull Request #{pull} · {owner}/{repo} - ", RegexOptions.Compiled);
        static readonly Regex windowTitleIssueRegex = new Regex($" · Issue #{issue} · {owner}/{repo} - ", RegexOptions.Compiled);
        static readonly Regex windowTitlePathRegex = new Regex($"{path} at {branch} · {owner}/{repo} - ", RegexOptions.Compiled);
        static readonly Regex windowTitleBranchesRegex = new Regex($"Branches · {owner}/{repo} - ", RegexOptions.Compiled);

        public GitHubContext FindContextFromUrl(UriString url)
        {
            return new GitHubContext
            {
                Host = url.Host,
                Owner = url.Owner,
                RepositoryName = url.RepositoryName,
            };
        }

        public GitHubContext FindContextFromBrowser()
        {
            return FindWindowTitlesForClass("Chrome_WidgetWin_1").Select(FindContextFromWindowTitle).Where(x => x != null).FirstOrDefault();
        }

        public IEnumerable<string> FindWindowTitlesForClass(string className = "Chrome_WidgetWin_1")
        {
            IntPtr handleWin = IntPtr.Zero;
            while (IntPtr.Zero != (handleWin = User32.FindWindowEx(IntPtr.Zero, handleWin, className, IntPtr.Zero)))
            {
                // Allocate correct string length first
                int length = User32.GetWindowTextLength(handleWin);
                if (length == 0)
                {
                    continue;
                }

                var titleBuilder = new StringBuilder(length + 1);
                User32.GetWindowText(handleWin, titleBuilder, titleBuilder.Capacity);
                yield return titleBuilder.ToString();
            }
        }

        public GitHubContext FindContextFromWindowTitle(string windowTitle)
        {
            var (success, owner, repo, branch, pullRequest, issue, path) = MatchWindowTitle(windowTitle);
            if (!success)
            {
                return null;
            }

            return new GitHubContext
            {
                Owner = owner,
                RepositoryName = repo,
                Branch = branch,
                PullRequest = pullRequest,
                Issue = issue,
                Path = path
            };
        }

        static (bool success, string owner, string repo, string branch, int? pullRequest, int? issue, string path) MatchWindowTitle(string windowTitle)
        {
            var match = windowTitlePathRegex.Match(windowTitle);
            if (match.Success)
            {
                return (match.Success, match.Groups["owner"].Value, match.Groups["repo"].Value, match.Groups["branch"].Value, null, null, match.Groups["path"].Value);
            }

            match = windowTitleRepositoryRegex.Match(windowTitle);
            if (match.Success)
            {
                return (match.Success, match.Groups["owner"].Value, match.Groups["repo"].Value, null, null, null, null);
            }

            match = windowTitleBranchRegex.Match(windowTitle);
            if (match.Success)
            {
                return (match.Success, match.Groups["owner"].Value, match.Groups["repo"].Value, match.Groups["branch"].Value, null, null, null);
            }

            match = windowTitleBranchesRegex.Match(windowTitle);
            if (match.Success)
            {
                return (match.Success, match.Groups["owner"].Value, match.Groups["repo"].Value, null, null, null, null);
            }

            match = windowTitlePullRequestRegex.Match(windowTitle);
            if (match.Success)
            {
                int.TryParse(match.Groups["pull"].Value, out int pullRequest);
                return (match.Success, match.Groups["owner"].Value, match.Groups["repo"].Value, null, pullRequest, null, null);
            }

            match = windowTitleIssueRegex.Match(windowTitle);
            if (match.Success)
            {
                int.TryParse(match.Groups["issue"].Value, out int issue);
                return (match.Success, match.Groups["owner"].Value, match.Groups["repo"].Value, null, null, issue, null);
            }

            return (match.Success, null, null, null, null, null, null);
        }

        static class User32
        {
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            internal static extern int GetWindowTextLength(IntPtr hWnd);
        }
    }
}
