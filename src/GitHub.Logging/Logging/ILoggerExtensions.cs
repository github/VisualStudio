using System;
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

        public static void FileAndForget(this Task task, ILogger log)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    log.Error(t.Exception, nameof(FileAndForget));
                }
            });
        }
    }
}