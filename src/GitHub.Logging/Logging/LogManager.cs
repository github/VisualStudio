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
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u4} [{ThreadId:00}] {ShortSourceContext,-25} {Message:lj}{NewLine}{Exception}";

            return new LoggerConfiguration()
                .Enrich.WithThreadId()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.File(logPath,
                    fileSizeLimitBytes: null,
                    outputTemplate: outputTemplate)
                .CreateLogger();
        }

        static Lazy<Logger> Logger { get; } = new Lazy<Logger>(CreateLogger);

        //Violates CA1004 - Generic methods should provide type parameter
#pragma warning disable CA1004
        public static ILogger ForContext<T>() => ForContext(typeof(T));
#pragma warning restore CA1004

        public static ILogger ForContext(Type type) => Logger.Value.ForContext(type).ForContext("ShortSourceContext", type.Name);
    }
}