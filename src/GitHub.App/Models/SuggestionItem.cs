using System;
using GitHub.Helpers;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a single auto completion suggestion (mentions, emojis, issues) in a generic format that can be
    /// easily cached.
    /// </summary>
    public class SuggestionItem
    {
        public SuggestionItem() // So this can be deserialized from cache
        {
        }

        public SuggestionItem(string name, Uri iconCacheKey)
        {
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            Ensure.ArgumentNotNull(iconCacheKey, "iconCacheKey");
            
            Name = name;
            IconKey = iconCacheKey;
        }

        public SuggestionItem(string name, string description)
        {
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            Ensure.ArgumentNotNullOrEmptyString(description, "description");

            Name = name;
            Description = description;
        }

        public SuggestionItem(string name, string description, Uri iconCacheKey)
        {
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            Ensure.ArgumentNotNull(iconCacheKey, "iconCacheKey");

            Name = name;
            Description = description;
            IconKey = iconCacheKey;
        }

        /// <summary>
        /// The name to display for this entry
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Additional details about the entry
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A key to lookup when displaying the icon for this entry
        /// </summary>
        public Uri IconKey { get; set; }

        /// <summary>
        /// The date this suggestion was last modified according to the API.
        /// </summary>
        public DateTimeOffset? LastModifiedDate { get; set; }
    }
}
