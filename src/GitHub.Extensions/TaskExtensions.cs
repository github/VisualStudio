using NullGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.Extensions
{
    public static class TaskExtensions
    {
        [return: AllowNull]
        public static async Task<T> Catch<T>(this Task<T> source, Func<Exception, T> handler)
        {
            return await source.ContinueWith<T>(t => handler(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
