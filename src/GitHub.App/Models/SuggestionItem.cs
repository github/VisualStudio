using System;
using GitHub.Extensions;
using GitHub.Helpers;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a single auto completion suggestion (mentions, emojis, issues) in a generic format that can be
    /// easily cached.
    /// </summary>
    public class SuggestionItem
    {
        public SuggestionItem(string name, string description)
        {
            Guard.ArgumentNotEmptyString(name, "name");
            Guard.ArgumentNotEmptyString(description, "description");

            Name = name;
            Description = description;
        }

        public SuggestionItem(string name, string description, string imageUrl)
        {
            Guard.ArgumentNotEmptyString(name, "name");

            Name = name;
            Description = description;
            ImageUrl = imageUrl;
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
        /// An image url for this entry
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// The date this suggestion was last modified according to the API.
        /// </summary>
        public DateTimeOffset? LastModifiedDate { get; set; }
    }
}
