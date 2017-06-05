using System;

namespace GitHub
{
    public static class GlobalCommands
    {
        public const string CommandSetString = "C5F1193E-F300-41B3-B4C4-5A703DD3C1C6";
        public const int ShowPullRequestCommentsId = 0x1000;
        public const int NextInlineCommentId = 0x1001;
        public const int PreviousInlineCommentId = 0x1002;
        public const int FirstInlineCommentId = 0x1003;

        public static readonly Guid CommandSetGuid = new Guid(CommandSetString);
    }
}
