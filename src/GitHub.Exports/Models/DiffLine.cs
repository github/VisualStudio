using System;

namespace GitHub.Models
{
    public class DiffLine
    {
        /// <summary>
        /// Was the line added, deleted or unchanged.
        /// </summary>
        public DiffChangeType Type { get; set; }

        /// <summary>
        /// Gets the old 1-based line number.
        /// </summary>
        public int OldLineNumber { get; set; } = -1;

        /// <summary>
        /// Gets the new 1-based line number.
        /// </summary>
        public int NewLineNumber { get; set; } = -1;

        /// <summary>
        /// Gets the unified diff line number where the first chunk header is line 0.
        /// </summary>
        public int DiffLineNumber { get; set; } = -1;

        /// <summary>
        /// Gets the content of the diff line (including +, - or space).
        /// </summary>
        public string Content { get; set; }

        public override string ToString() => Content;
    }
}
