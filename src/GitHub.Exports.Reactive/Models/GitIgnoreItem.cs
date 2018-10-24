using GitHub.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class GitIgnoreItem : ICopyable<GitIgnoreItem>, IEquatable<GitIgnoreItem>
    {
        static readonly string[] recommendedIgnoreFiles = { "None", "VisualStudio", "Node", "Eclipse", "C++", "Windows" };

        static readonly GitIgnoreItem none = new GitIgnoreItem("None");
        public static GitIgnoreItem None { get { return none; } }

        public static GitIgnoreItem Create(string name)
        {
            return name.Equals("None", StringComparison.OrdinalIgnoreCase) ? none : new GitIgnoreItem(name);
        }

        GitIgnoreItem(string name)
        {
            Name = name;
            Recommended = IsRecommended(name);
        }

        public void CopyFrom(GitIgnoreItem other)
        {
            Name = other.Name;
            Recommended = other.Recommended;
        }

        public string Name { get; private set; }

        public bool Recommended { get; private set; }

        public static bool IsRecommended(string name)
        {
            return recommendedIgnoreFiles.Any(item => item.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        bool IEquatable<GitIgnoreItem>.Equals(GitIgnoreItem other) => Name == other.Name;

        public override bool Equals(object obj)
        {
            var item = obj as GitIgnoreItem;
            return item != null && Name == item.Name;
        }

        public override int GetHashCode() => 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);

        internal string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "GitIgnore: {0} Recommended: {1}", Name, Recommended);
            }
        }
    }
}
