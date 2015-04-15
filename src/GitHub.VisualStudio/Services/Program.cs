using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using GitHub.Models;
using Octokit;

namespace GitHub.VisualStudio
{
    // Represents the currently executing program.
    [Export(typeof(IProgram))]
    public class Program : IProgram
    {
        public Program()
        {
            applicationName = Info.ApplicationInfo.ApplicationName;
            applicationDescription = Info.ApplicationInfo.ApplicationDescription;

            var executingAssembly = typeof(Program).Assembly;
            AssemblyName = executingAssembly.GetName();
            ExecutingAssemblyDirectory = Path.GetDirectoryName(executingAssembly.Location);
            ProductHeader = new ProductHeaderValue(Info.ApplicationInfo.ApplicationSafeName, AssemblyName.Version.ToString());
        }

        readonly string applicationName;
        readonly string applicationDescription;

        /// <summary>
        /// Name of this application
        /// </summary>
        public string ApplicationName { get { return applicationName; } }

        /// <summary>
        /// Name of this application
        /// </summary>
        public string ApplicationDescription { get { return applicationDescription; } }

        /// <summary>
        /// The currently executing assembly.
        /// </summary>
        public AssemblyName AssemblyName { get; private set; }

        /// <summary>
        /// The directory that contains the currently executing assembly.
        /// </summary>
        public string ExecutingAssemblyDirectory { get; private set; }

        /// <summary>
        /// The product header used in the user agent.
        /// </summary>
        public ProductHeaderValue ProductHeader { get; private set; }
    }
}