using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    /// <summary>
    /// Model for a single check suite.
    /// </summary>
    public class CheckSuiteModel
    {
        /// <summary>
        /// The conclusion of this check suite.
        /// </summary>
        public CheckConclusionState? Conclusion { get; set; }

        /// <summary>
        /// The status of this check suite.
        /// </summary>
        public CheckStatusState Status { get; set; }

        /// <summary>
        /// Identifies the date and time when the object was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Identifies the date and time when the object was last updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// The check runs associated with a check suite.
        /// </summary>
        public List<CheckRunModel> CheckRuns { get; set; }
    }
}