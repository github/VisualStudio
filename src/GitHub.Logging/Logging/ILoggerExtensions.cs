using System;
using System.Globalization;
using System.Threading.Tasks;
using Serilog;

namespace GitHub.Logging
{
    public static class ILoggerExtensions
    {
        public static void Assert(this ILogger logger, bool condition, string messageTemplate)
        {
            if (!condition)
            {
                messageTemplate = "Assertion Failed: " + messageTemplate;
#pragma warning disable Serilog004 // propertyValues might not be strings
                logger.Warning(messageTemplate);
#pragma warning restore Serilog004
            }
        }

        public static void Assert(this ILogger logger, bool condition, string messageTemplate, params object[] propertyValues)
        {
            if (!condition)
            {
                messageTemplate = "Assertion Failed: " + messageTemplate;
#pragma warning disable Serilog004 // propertyValues might not be strings
                logger.Warning(messageTemplate, propertyValues);
#pragma warning restore Serilog004
            }
        }

        public static void Time(this ILogger logger, string name, Action method)
        {
            var startTime = DateTime.Now;
            method();
            logger.Verbose("{Name} took {Seconds} seconds", name, FormatSeconds(DateTime.Now - startTime));
        }

        public static T Time<T>(this ILogger logger, string name, Func<T> method)
        {
            var startTime = DateTime.Now;
            var value = method();
            logger.Verbose("{Name} took {Seconds} seconds", name, FormatSeconds(DateTime.Now - startTime));
            return value;
        }

        public static async Task TimeAsync(this ILogger logger, string name, Func<Task> methodAsync)
        {
            var startTime = DateTime.Now;
            await methodAsync().ConfigureAwait(false);
            logger.Verbose("{Name} took {Seconds} seconds", name, FormatSeconds(DateTime.Now - startTime));
        }

        public static async Task<T> TimeAsync<T>(this ILogger logger, string name, Func<Task<T>> methodAsync)
        {
            var startTime = DateTime.Now;
            var value = await methodAsync().ConfigureAwait(false);
            logger.Verbose("{Name} took {Seconds} seconds", name, FormatSeconds(DateTime.Now - startTime));
            return value;
        }

        static string FormatSeconds(TimeSpan timeSpan) => timeSpan.TotalSeconds.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
