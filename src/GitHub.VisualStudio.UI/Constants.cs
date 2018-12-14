#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace GitHub.VisualStudio.UI
{
    public static class Constants
    {
        public const string NoAngleBracketsErrorMessage = "Failed to parse signature - Neither `name` nor `email` should contain angle brackets chars.";
        public const int MaxRepositoryNameLength = 100;
        public const int MaxDirectoryLength = 200; // Windows allows 248, but we need to allow room for subdirectories.
        public const int MaxFilePathLength = 260;

        public const string Notification_RepoCreated = "[{0}](u:{1}) has been successfully created.";
        public const string Notification_RepoCloned = "[{0}](u:{1}) has been successfully cloned.";
        public const string Notification_CreateNewProject = "[Create a new project or solution](c:{0}).";
        public const string Notification_OpenProject = "[Open an existing project or solution](o:{0}).";
    }
}
