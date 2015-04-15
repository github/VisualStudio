namespace GitHub.Info
{
    public static class ApplicationInfo
    {
#if DEBUG
        public const string ApplicationName = "GìtHūbForVisualStudio";
        public const string ApplicationProvider = "GitHub";
#else
        public const string ApplicationName = "GitHubForVisualStudio";
        public const string ApplicationProvider = "GitHub";
#endif
        public const string ApplicationSafeName = "GitHubForVisualStudio";
        public const string ApplicationDescription = "GitHub Extension for Visual Studio";
    }
}
