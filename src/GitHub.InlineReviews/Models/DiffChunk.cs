using System;
using System.Collections.Generic;
using System.Text;

namespace GitHub.InlineReviews.Models
{
    public class DiffChunk
    {
        public int OldLineNumber { get; set; }
        public int NewLineNumber { get; set; }
        public IList<DiffLine> Lines { get; } = new List<DiffLine>();

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var line in Lines)
            {
                builder.AppendLine(line.Content);
            }

            return builder.ToString();
        }
    }
}
