using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using GitHub.Info;
using Serilog;
using Serilog.Core;

namespace GitHub.Logging
{
    public static class LogManager
    {
#if DEBUG
        private static LogEventLevel DefaultLoggingLevel = LogEventLevel.Debug;
#else
        private static LogEventLevel DefaultLoggingLevel = LogEventLevel.Information;
#endif

        private static LoggingLevelSwitch LoggingLevelSwitch = new LoggingLevelSwitch(DefaultLoggingLevel);

        static Logger CreateLogger()
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ApplicationInfo.ApplicationName,
                "extension.log");

            const string outputTemplate =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u4} [{ThreadId:00}] {ShortSourceContext,-25} {Message:lj}{NewLine}{Exception}";

            return new LoggerConfiguration()
                .Enrich.WithThreadId()
                .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                .WriteTo.File(logPath,
                    fileSizeLimitBytes: null,
                    outputTemplate: outputTemplate)
                .CreateLogger();
        }

        public static void EnableTraceLogging(bool enable)
        {
            Logger.Value.ForContext(typeof(LogManager)).Information("EnableTraceLogging: {Enable}", enable);

            var logEventLevel = enable ? LogEventLevel.Verbose : DefaultLoggingLevel;
            if(LoggingLevelSwitch.MinimumLevel != logEventLevel)
            { 
                Logger.Value.ForContext(typeof(LogManager)).Information("Logging Level: {LogEventLevel}", logEventLevel);
                LoggingLevelSwitch.MinimumLevel = logEventLevel;
            }
        }

        static Lazy<Logger> Logger { get; } = new Lazy<Logger>(CreateLogger);

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static ILogger ForContext<T>() => ForContext(typeof(T));

        public static ILogger ForContext(Type type) => Logger.Value.ForContext(type).ForContext("ShortSourceContext", type.Name);
    }
}