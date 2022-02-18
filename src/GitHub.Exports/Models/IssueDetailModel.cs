using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    /// <summary>
    /// Holds the details of an issue.
    /// </summary>
    public class IssueDetailModel : IssueishDetailModel
    {
        /// <summary>
        /// Gets or sets the issue comments.
        /// </summary>
        public IReadOnlyList<CommentModel> Comments { get; set; }
    }
}
