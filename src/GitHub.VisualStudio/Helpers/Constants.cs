namespace GitHub.VisualStudio.Helpers
{
    public static class Constants
    {
        public const string NoAngleBracketsErrorMessage = "Failed to parse signature - Neither `name` nor `email` should contain angle brackets chars.";
        public const int MaxRepositoryNameLength = 100;
        public const int MaxDirectoryLength = 200; // Windows allows 248, but we need to allow room for subdirectories.
        public const int MaxFilePathLength = 260;
    }
}
