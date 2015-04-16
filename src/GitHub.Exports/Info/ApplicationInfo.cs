namespace GitHub.Info
{
    public static class ApplicationInfo
    {
#if DEBUG
        public const string ApplicationName = "GìtHūbVisualStudio";
        public const string ApplicationProvider = "GitHub";
#else
        public const string ApplicationName = "GitHubVisualStudio";
        public const string ApplicationProvider = "GitHub";
#endif
        public const string ApplicationSafeName = "GitHubVisualStudio";
        public const string ApplicationDescription = "GitHub Extension for Visual Studio";
    }
}
