using System;

namespace GitHub.Models
{
    /// <summary>
    /// Wraps a <see cref="UsageModel"/> with a <see cref="LastUpdated"/> field.
    /// </summary>
    public class UsageData
    {
        /// <summary>
        /// Gets or sets the last update time.
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; }

        /// <summary>
        /// Gets the model containing the current usage data.
        /// </summary>
        public UsageModel Model { get; set; }
    }
}
