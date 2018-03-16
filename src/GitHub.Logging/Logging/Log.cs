using System;
using Serilog;

namespace GitHub.Logging
{
    public static class Log
    {
        private static Lazy<ILogger> Logger { get; } = new Lazy<ILogger>(() => LogManager.ForContext(typeof(Log)));

        public static void Assert(bool condition, string messageTemplate)
            => Logger.Value.Assert(condition, messageTemplate);
    }
}