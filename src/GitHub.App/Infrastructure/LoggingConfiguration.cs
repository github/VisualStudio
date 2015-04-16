using GitHub.Extensions;
using GitHub.Models;
using NLog;
using NLog.Config;
using NLog.Targets;
using Rothko;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public LoggingConfiguration(IProgram program, IOperatingSystem os)
        {
            string assemblyFolder = program.ExecutingAssemblyDirectory;
            var conf = new XmlLoggingConfiguration(System.IO.Path.Combine(assemblyFolder, "NLog.config"), true);
            var fileTarget = new FileTarget();
            conf.AddTarget("file", fileTarget);
            fileTarget.FileName = Path.Combine(os.Environment.GetLocalGitHubApplicationDataPath(), "extension.log");
            fileTarget.Layout = "${message}";
            var rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
            conf.LoggingRules.Add(rule);
            NLog.LogManager.Configuration = conf;
        }

        public void Configure()
        {
        }
    }
}
