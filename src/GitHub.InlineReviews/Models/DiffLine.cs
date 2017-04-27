using System;

namespace GitHub.InlineReviews.Models
{
    public class DiffLine
    {
        public DiffChangeType Type { get; set; }
        public int OldLineNumber { get; set; } = -1;
        public int NewLineNumber { get; set; } = -1;
        public int DiffLineNumber { get; set; } = -1;
        public string Content { get; set; }

        public override string ToString() => Content;
    }
}
