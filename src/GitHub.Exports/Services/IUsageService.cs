using System;
using System.Threading.Tasks;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Provides services for <see cref="IUsageTracker"/>.
    /// </summary>
    public interface IUsageService
    {
        /// <summary>
        /// Gets a GUID that anonymously represents the user.
        /// </summary>
        Task<Guid> GetUserGuid();

        /// <summary>
        /// Starts a timer.
        /// </summary>
        /// <param name="callback">The callback to call when the timer ticks.</param>
        /// <param name="dueTime">The timespan after which the callback will be called the first time.</param>
        /// <param name="period">The timespan after which the callback will be called subsequent times.</param>
        /// <returns>A disposable used to cancel the timer.</returns>
        IDisposable StartTimer(Func<Task> callback, TimeSpan dueTime, TimeSpan period);

        /// <summary>
        /// Reads the local usage data from disk.
        /// </summary>
        /// <returns>A task returning a <see cref="UsageData"/> object.</returns>
        Task<UsageData> ReadLocalData();

        /// <summary>
        /// Writes the local usage data to disk.
        /// </summary>
        Task WriteLocalData(UsageData data);
    }
}
