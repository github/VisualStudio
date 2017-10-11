using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    /// <summary>
    /// Holds a collection of <see cref="UsageData"/> daily usage reports.
    /// </summary>
    public class UsageData
    {
        /// <summary>
        /// Gets a list of unsent daily usage reports.
        /// </summary>
        public List<UsageModel> Reports { get; set; }
    }
}
