using System;

namespace GitHub.Models
{
    /// <summary>
    /// Holds the details of a commit.
    /// </summary>
    public class CommitModel
    {
        /// <summary>
        /// Gets the abbreviated git object ID for the commit.
        /// </summary>
        public string AbbreviatedOid { get; set; }

        /// <summary>
        /// Gets the commit headline.
        /// </summary>
        public string MessageHeadline { get; set; }
    }
}
