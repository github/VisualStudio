using System.Reflection;
using Octokit;

namespace GitHub.Models
{
    // Represents the currently executing program.
    public interface IProgram
    {
        AssemblyName AssemblyName { get; }
        string ExecutingAssemblyDirectory { get; }
        ProductHeaderValue ProductHeader { get; }
    }
}
