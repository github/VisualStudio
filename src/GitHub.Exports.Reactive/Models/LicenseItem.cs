using System.Globalization;
using Octokit;
using GitHub.Collections;
using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public class LicenseItem : ICopyable<LicenseItem>, IEquatable<LicenseItem>
    {
        static readonly LicenseItem none = new LicenseItem();
        public static LicenseItem None { get { return none; } }

        public LicenseItem(string key, string name)
        {
            Key = key;
            Name = name;
            Recommended = IsRecommended(key);
        }

        LicenseItem()
        {
            Key = "";
            Name = "None";
            Recommended = true;
        }

        public static bool IsRecommended(string licenseKey)
        {
            return licenseKey == "mit" || licenseKey == "gpl-2.0" || licenseKey == "apache-2.0";
        }

        public void CopyFrom(LicenseItem other)
        {
            Key = other.Key;
            Name = other.Name;
            Recommended = other.Recommended;
        }

        bool IEquatable<LicenseItem>.Equals(LicenseItem other) => Key == other.Key;

        public override bool Equals(object obj)
        {
            var item = obj as LicenseItem;
            return item != null && Key == item.Key;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Key);
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
