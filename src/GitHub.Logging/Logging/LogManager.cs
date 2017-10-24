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
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}|{Level}|Thread:{ThreadId}|{SourceContext}|{Message:lj}{NewLine}{Exception}";

            return new LoggerConfiguration()
                .Enrich.WithThreadId()
                .MinimumLevel.Information()
                .WriteTo.File(logPath,
                    fileSizeLimitBytes: null,
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
}