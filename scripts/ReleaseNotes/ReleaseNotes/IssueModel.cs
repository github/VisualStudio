using System;
using System.Collections.Generic;
using System.Linq;

namespace GhfvsReleaseNotes
{
    internal class IssueModel
    {
        public static readonly string[] InterestingLabels = new[] { "feature", "enhancement", "bug" };
        public static readonly IList<string> Sections = new[] { "Feature", "Enhancement", "Fixes" };
        static readonly IList<string> Badges = new[] { "Added", "Improved", "Fixed" };

        public int Number { get; set; }
        public string Title { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public IList<string> Labels { get; internal set; }
        public string Badge => Badges[LabelIndex];
        public string Section => Sections[LabelIndex];
        public int SortOrder => LabelIndex;
        int LabelIndex => Labels.Select(x => InterestingLabels.ToList().IndexOf(x)).FirstOrDefault(x => x != -1);
    }
}