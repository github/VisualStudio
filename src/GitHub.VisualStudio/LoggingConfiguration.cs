using Serilog;

namespace GitHub.VisualStudio
{
    public static class LoggingConfiguration
    {
        static bool initialized; 

        public static void InitializeLogging(string logPath)
        {
            if(initialized)
                return;

            initialized = true;

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .WriteTo.File(logPath,
                    fileSizeLimitBytes: 2L*1024L*1024L,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}|{Level}|Thread:{ThreadId}|{SourceContext}|{Message}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
}