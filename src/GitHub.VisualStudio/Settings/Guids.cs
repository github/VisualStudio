using System;

namespace GitHub.VisualStudio
{
    static class GuidList
    {
        public const string guidGitHubPkgString = "c3d3dc68-c977-411f-b3e8-03b0dccf7dfc";
        public const string guidAssemblyResolverPkgString = "a6424dba-34cb-360d-a4de-1b0b0411e57d";
        public const string guidGitHubCmdSetString = "c4c91892-8881-4588-a5d9-b41e8f540f5a";
        public const string guidGitHubToolbarCmdSetString = "C5F1193E-F300-41B3-B4C4-5A703DD3C1C6";
        public const string guidContextMenuSetString = "31057D08-8C3C-4C5B-9F91-8682EA08EC27";
        public const string guidImageMonikerString = "27841f47-070a-46d6-90be-a5cbbfc724ac";

        public static readonly Guid guidGitHubCmdSet = new Guid(guidGitHubCmdSetString);
        public static readonly Guid guidGitHubToolbarCmdSet = new Guid(guidGitHubToolbarCmdSetString);
        public static readonly Guid guidContextMenuSet = new Guid(guidContextMenuSetString);
        public static readonly Guid guidImageMoniker = new Guid(guidImageMonikerString);
    }
}
