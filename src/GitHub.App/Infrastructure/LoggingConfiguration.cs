using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using Rothko;
using Serilog;

namespace GitHub.Infrastructure
{
    public interface ILoggingConfiguration
    {
        void Configure();
    }

    [Export(typeof(ILoggingConfiguration))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LoggingConfiguration : ILoggingConfiguration
    {
        [ImportingConstructor]
        public LoggingConfiguration(IProgram program, IOperatingSystem os, IVSServices vsservice)
        {
            try
            {
                var logPath = Path.Combine(os.Environment.GetLocalGitHubApplicationDataPath(), "extension.log");

                Log.Logger = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .WriteTo.RollingFile(logPath,
                        fileSizeLimitBytes: 2L * 1024L * 1024L, 
                        retainedFileCountLimit: 3,
                        buffered: true,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}|{Level}|Thread:{ThreadId}|{SourceContext}|{Message}{NewLine}{Exception}")
                    .CreateLogger();
            }
            catch (Exception ex)
            {
                vsservice.ActivityLogError(string.Format(CultureInfo.InvariantCulture, "Error configuring the log. {0}", ex));
            }
        }

        public void Configure()
        {
        }
    }
}
