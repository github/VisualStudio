using System.Globalization;
using Octokit;

namespace GitHub.Models
{
    public class LicenseItem
    {
        public static LicenseItem None = new LicenseItem();

        public LicenseItem(LicenseMetadata license)
        {
            Key = license.Key;
            Name = license.Name;
            Recommended = license.Key == "mit" || license.Key == "gpl-2.0" || license.Key == "apache-2.0";
        }

        LicenseItem()
        {
            Key = "";
            Name = "None";
            Recommended = true;
        }

        public string Key { get; private set; }

        public string Name { get; private set; }

        public bool Recommended { get; private set; }

        internal string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "License: {0} Recommended: {1}", Key, Recommended);
            }
        }
    }
}
