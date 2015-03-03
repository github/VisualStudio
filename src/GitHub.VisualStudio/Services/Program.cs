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
            ProductHeader = new ProductHeaderValue("GitHubVS", AssemblyName.Version.ToString());
        }

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