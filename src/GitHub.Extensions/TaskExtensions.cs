using System;
using System.Threading.Tasks;
using NullGuard;

namespace GitHub.Extensions
{
    public static class TaskExtensions
    {
        [return: AllowNull]
        public static async Task<T> Catch<T>(this Task<T> source, Func<Exception, T> handler = null)
        {
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

        [return: AllowNull]
        public static async Task Catch(this Task source, Action<Exception> handler = null)
        {
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
        public static void Forget([AllowNull] this Task task)
        {
        }
    }
}
