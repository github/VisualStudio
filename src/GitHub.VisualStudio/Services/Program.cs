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
            var executingAssembly = typeof(Program).Assembly;
            AssemblyName = executingAssembly.GetName();
            ExecutingAssemblyDirectory = Path.GetDirectoryName(executingAssembly.Location);
            ProductHeader = new ProductHeaderValue("GitHubVisualStudio", AssemblyName.Version.ToString());
        }

        const string applicationProvider = "GitHub";
        const string applicationName = "GitHub Extension for Visual Studio";

        /// <summary>
        /// Provider of this application
        /// </summary>
        public string ApplicationProvider {  get { return ApplicationProvider; } }

        /// <summary>
        /// Name of this application
        /// </summary>
        public string ApplicationName { get { return applicationName; } }

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