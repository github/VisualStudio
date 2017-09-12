using System;
using System.IO;
using GitHub.Info;
using Serilog;
using Serilog.Core;

namespace GitHub.Logging
{
    public static class LogManager
    {
        static Logger CreateLogger()
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ApplicationInfo.ApplicationName,
                "extension.log");

            const string outputTemplate =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}|{Level}|Thread:{ThreadId}|{SourceContext}|{Message}{NewLine}{Exception}";

            //2MBs
            const long fileSizeLimitBytes = 2L * 1024L * 1024L;

            return new LoggerConfiguration()
                .Enrich.WithThreadId()
                .WriteTo.File(logPath,
                    fileSizeLimitBytes: fileSizeLimitBytes,
                    outputTemplate: outputTemplate)
                .CreateLogger();
        }

        static Lazy<Logger> Logger { get; } = new Lazy<Logger>(CreateLogger);

        //Violates CA1004 - Generic methods should provide type parameter
#pragma warning disable CA1004
        public static ILogger ForContext<T>() => Logger.Value.ForContext<T>();
#pragma warning restore CA1004

        public static ILogger ForContext(Type type) => Logger.Value.ForContext(type);
    }

    public static class Log
    {
        private static Lazy<ILogger> Logger { get; } = new Lazy<ILogger>(() => LogManager.ForContext(typeof(Log)));

        public static void Information(string messageTemplate)
            => Logger.Value.Information(messageTemplate);

        public static void Information(string messageTemplate, params object[] propertyValues)
            => Logger.Value.Information(messageTemplate, propertyValues);

        public static void Information(Exception exception, string messageTemplate)
            => Logger.Value.Information(exception, messageTemplate);

        public static void Information(Exception exception, string messageTemplate, params object[] propertyValues)
            => Logger.Value.Information(exception, messageTemplate, propertyValues);

        public static void Debug(string messageTemplate)
            => Logger.Value.Debug(messageTemplate);

        public static void Debug(string messageTemplate, params object[] propertyValues)
            => Logger.Value.Debug(messageTemplate, propertyValues);

        public static void Debug(Exception exception, string messageTemplate)
            => Logger.Value.Debug(exception, messageTemplate);

        public static void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
            => Logger.Value.Debug(exception, messageTemplate, propertyValues);

        public static void Error(string messageTemplate)
            => Logger.Value.Error(messageTemplate);

        public static void Error(string messageTemplate, params object[] propertyValues)
            => Logger.Value.Error(messageTemplate, propertyValues);

        public static void Error(Exception exception, string messageTemplate)
            => Logger.Value.Error(exception, messageTemplate);

        public static void Error(Exception exception, string messageTemplate, params object[] propertyValues)
            => Logger.Value.Error(exception, messageTemplate, propertyValues);

        public static void Fatal(Exception exception, string messageTemplate)
            => Logger.Value.Fatal(exception, messageTemplate);

        public static void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
            => Logger.Value.Fatal(exception, messageTemplate, propertyValues);

        public static void Fatal(string messageTemplate)
            => Logger.Value.Fatal(messageTemplate);

        public static void Fatal(string messageTemplate, params object[] propertyValues)
            => Logger.Value.Fatal(messageTemplate, propertyValues);

        public static void Verbose(string messageTemplate)
            => Logger.Value.Verbose(messageTemplate);

        public static void Verbose(string messageTemplate, params object[] propertyValues)
            => Logger.Value.Verbose(messageTemplate, propertyValues);

        public static void Verbose(Exception exception, string messageTemplate)
            => Logger.Value.Verbose(exception, messageTemplate);

        public static void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
            => Logger.Value.Verbose(exception, messageTemplate, propertyValues);

        public static void Warning(string messageTemplate)
            => Logger.Value.Warning(messageTemplate);

        public static void Warning(string messageTemplate, params object[] propertyValues)
            => Logger.Value.Warning(messageTemplate, propertyValues);

        public static void Warning(Exception exception, string messageTemplate)
            => Logger.Value.Warning(exception, messageTemplate);

        public static void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
            => Logger.Value.Warning(exception, messageTemplate, propertyValues);

        public static void Assert(bool condition, string messageTemplate)
            => Logger.Value.Assert(condition, messageTemplate);

        public static void Assert(bool condition, string messageTemplate, params object[] propertyValues)
            => Logger.Value.Assert(condition, messageTemplate, propertyValues);
    }

    public static class ILoggerExtensions
    {
        public static void Assert(this ILogger logger, bool condition, string messageTemplate)
        {
            if (!condition)
            {
                messageTemplate = "Assertion Failed: " + messageTemplate;
                logger.Warning(messageTemplate);
            }
        }

        public static void Assert(this ILogger logger, bool condition, string messageTemplate, params object[] propertyValues)
        {
            if (!condition)
            {
                messageTemplate = "Assertion Failed: " + messageTemplate;
                logger.Warning(messageTemplate, propertyValues);
            }
        }
    }
}