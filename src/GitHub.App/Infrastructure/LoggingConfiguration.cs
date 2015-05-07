using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using NLog;
using NLog.Config;
using NLog.Targets;
using Rothko;

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
        // See http://nlog-project.org/wiki/Layout_Renderers for logging layout options.
        const string layout = "${longdate}|${level:uppercase=true}|thread:${threadid:padding=2}|${logger:shortName=true}|${message}${onexception:inner=${newline}${exception:innerformat=ToString,StackTrace:format=ToString,StackTrace}}";

        [ImportingConstructor]
        public LoggingConfiguration(IProgram program, IOperatingSystem os, IVSServices vsservice)
        {
            NLog.Config.LoggingConfiguration conf;
            string assemblyFolder = program.ExecutingAssemblyDirectory;
            try
            {
                conf = new XmlLoggingConfiguration(Path.Combine(assemblyFolder, "NLog.config"), true);
            }
            catch (Exception ex)
            {
                vsservice.ActivityLogError(string.Format(CultureInfo.InvariantCulture, "Error loading nlog.config. {0}", ex));
                conf = new NLog.Config.LoggingConfiguration();
            }

            var fileTarget = conf.FindTargetByName("file") as FileTarget;
            if (fileTarget == null)
            {
                fileTarget = new FileTarget();
                conf.AddTarget(Path.GetRandomFileName(), fileTarget);
                conf.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, fileTarget));
            }
            fileTarget.FileName = Path.Combine(os.Environment.GetLocalGitHubApplicationDataPath(), "extension.log");
            fileTarget.Layout = layout;

            try
            {
                LogManager.Configuration = conf;
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
