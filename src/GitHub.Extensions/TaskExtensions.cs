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
        public static void Forget(this Task task)
        {
        }
    }
}
