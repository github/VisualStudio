using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace GitHub.Models
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class GitIgnoreItem
    {
        readonly string[] recommendedIgnoreFiles = { "None", "VisualStudio", "Node", "Eclipse", "C++", "Windows" };

        static readonly GitIgnoreItem none = new GitIgnoreItem("None");
        public static GitIgnoreItem None { get { return none; } }

        public static GitIgnoreItem Create(string name)
        {
            return name.Equals("None", StringComparison.OrdinalIgnoreCase) ? none : new GitIgnoreItem(name);
        }

        GitIgnoreItem(string name)
        {
            Name = name;
            Recommended = recommendedIgnoreFiles.Any(item => item.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }

        public string Name { get; private set; }

        public bool Recommended { get; private set; }

        internal string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "GitIgnore: {0} Recommended: {1}", Name, Recommended);
            }
        }
    }
}
