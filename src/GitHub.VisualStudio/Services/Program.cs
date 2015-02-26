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

        public AssemblyName AssemblyName { get; private set; }
        public string ExecutingAssemblyDirectory { get; private set; }
        public ProductHeaderValue ProductHeader { get; private set; }
    }
}