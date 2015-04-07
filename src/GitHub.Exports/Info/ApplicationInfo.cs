namespace GitHub.Info
{
    public static class ApplicationInfo
    {
#if DEBUG
        public const string ApplicationName = "GitHubForVisualStudio_Debug";
        public const string ApplicationProvider = "GitHub_Debug";
#else
        public const string ApplicationName = "GitHubForVisualStudio";
        public const string ApplicationProvider = "GitHub";
#endif
        public const string ApplicationDescription = "GitHub Extension for Visual Studio";
    }
}
