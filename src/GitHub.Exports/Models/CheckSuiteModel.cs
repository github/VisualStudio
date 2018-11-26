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
        /// The head sha of a Check Suite.
        /// </summary>
        public string HeadSha { get; set; }

        /// <summary>
        /// The check runs associated with a check suite.
        /// </summary>
        public List<CheckRunModel> CheckRuns { get; set; }

        public string ApplicationName { get; set; }
    }
}