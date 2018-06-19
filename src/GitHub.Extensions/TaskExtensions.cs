using System;
using System.Threading.Tasks;
using Serilog;

namespace GitHub.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<T> Catch<T>(this Task<T> source, Func<Exception, T> handler = null)
        {
            Guard.ArgumentNotNull(source, nameof(source));

            try
            {
                return await source;
            }
            catch (Exception ex)
            {
                if (handler != null)
                    return handler(ex);
                return default(T);
            }
        }

        public static async Task Catch(this Task source, Action<Exception> handler = null)
        {
            Guard.ArgumentNotNull(source, nameof(source));

            try
            {
                await source;
            }
            catch (Exception ex)
            {
                if (handler != null)
                    handler(ex);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "task")]
        public static void Forget(this Task task)
        {
        }

        /// <summary>
        /// Log any exceptions when a task throws.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to log exceptions from.</param>
        /// <param name="log">The <see cref="ILogger"/> to use.</param>
        public static void LogAndForget(this Task task, ILogger log)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    log.Error(t.Exception, nameof(LogAndForget));
                }
            });
        }
    }
}
