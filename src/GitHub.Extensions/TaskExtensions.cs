using System;
using System.Threading.Tasks;

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
    }
}
