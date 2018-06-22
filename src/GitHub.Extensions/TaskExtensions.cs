using System;
using System.Threading.Tasks;
using GitHub.Logging;
using Serilog;

namespace GitHub.Extensions
{
    public static class TaskExtensions
    {
        static readonly ILogger log = LogManager.ForContext(typeof(TaskExtensions));

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

        /// <summary>
        /// Allow task to run and log any exceptions.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to log exceptions from.</param>
        /// <param name="errorMessage">An error message to log if the task throws.</param>
        public static void Forget(this Task task, string errorMessage = "")
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    log.Error(t.Exception, errorMessage);
                }
            });
        }

        /// <summary>
        /// Allow task to run and log any exceptions.
        /// </summary>
        /// <param name="task">The task to log exceptions from.</param>
        /// <param name="log">The logger to use.</param>
        /// <param name="errorMessage">The error message to log if the task throws.</param>
        public static void Forget(this Task task, ILogger log, string errorMessage = "")
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    log.Error(t.Exception, errorMessage);
                }
            });
        }
    }
}
