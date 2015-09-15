// Guids.cs
// MUST match guids.h
using Microsoft.TeamFoundation.Controls;
using System;

namespace GitHub.VisualStudio
{
    static class GuidList
    {
        public const string guidGitHubPkgString = "c3d3dc68-c977-411f-b3e8-03b0dccf7dfc";
        public const string guidGitHubCmdSetString = "c4c91892-8881-4588-a5d9-b41e8f540f5a";
        public const string guidGitHubToolbarCmdSetString = "C5F1193E-F300-41B3-B4C4-5A703DD3C1C6";

        public static readonly Guid guidGitHubCmdSet = new Guid(guidGitHubCmdSetString);
        public static readonly Guid guidGitHubToolbarCmdSet = new Guid(guidGitHubToolbarCmdSetString);
    }

    static class NavigationItemPriority
    {
        public const int PullRequests = TeamExplorerNavigationItemPriority.GitCommits - 1;
        public const int Wiki = TeamExplorerNavigationItemPriority.Settings - 1;
        public const int Pulse = Wiki - 3;
        public const int Graphs = Wiki - 2;
        public const int Issues = Wiki - 1;
    }
}
