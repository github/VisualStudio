using System;

namespace GitHub.VisualStudio
{
    public static class Guids
    {
        public const string PackageId = "c3d3dc68-c977-411f-b3e8-03b0dccf7dfc";
        public const string ImagesId = "27841f47-070a-46d6-90be-a5cbbfc724ac";
        public const string UsageTrackerId = "9362DD38-7E49-4B5D-9DE1-E843D4155716";
        public const string UIProviderId = "304F2186-17C4-4C66-8A54-5C96F9353A28";
        public const string GitHubServiceProviderId = "76909E1A-9D58-41AB-8957-C26B9550787B";
        public const string StartPagePackageId = "3b764d23-faf7-486f-94c7-b3accc44a70e";
        public const string CodeContainerProviderId = "6CE146CB-EF57-4F2C-A93F-5BA685317660";
        public const string InlineReviewsPackageId = "248325BE-4A2D-4111-B122-E7D59BF73A35";
        public const string PullRequestStatusPackageId = "5121BEC6-1088-4553-8453-0DDC7C8E2238";
        public const string GitHubPanePackageId = "0A40459D-6B6D-4110-B6CE-EC83C0BC6A09";
        public const string TeamExplorerWelcomeMessage = "C529627F-8AA6-4FDB-82EB-4BFB7DB753C3";
        public const string LoginManagerId = "7BA2071A-790A-4F95-BE4A-0EEAA5928AAF";

        // VisualStudio IDs
        public const string GitSccProviderId = "11B8E6D7-C08B-4385-B321-321078CDD1F8";
        public const string TeamExplorerInstall3rdPartyGitTools = "DF785C7C-8454-4836-9686-D1C4A01D0BB9";

        // UIContexts
        public const string UIContext_Git = "565515AD-F4C1-4D59-BC14-AE77396DDDD7";

        // Guids defined in GitHub.VisualStudio.vsct
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

        // Guids defined in InlineReviewsPackage.vsct
        public const string CommandSetString = "C5F1193E-F300-41B3-B4C4-5A703DD3C1C6";
        public static readonly Guid CommandSetGuid = new Guid(CommandSetString);

        // Callout notification IDs
        public static readonly Guid NoRemoteOriginCalloutId = new Guid("B5679412-58A1-49CD-96E9-8F093FE3DC79");
    }
}
