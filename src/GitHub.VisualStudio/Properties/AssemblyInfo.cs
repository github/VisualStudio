using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

[assembly: AssemblyTitle("GitHub.VisualStudio")]
[assembly: AssemblyDescription("GitHub for Visual Studio VSPackage")]
[assembly: Guid("fad77eaa-3fe1-4c4b-88dc-3753b6263cd7")]

[assembly: ProvideCodeBase(AssemblyName = "GitHub.UI", CodeBase = @"$PackageFolder$\GitHub.UI.dll")]
[assembly: ProvideCodeBase(AssemblyName = "GitHub.VisualStudio.UI", CodeBase = @"$PackageFolder$\GitHub.VisualStudio.UI.dll")]
[assembly: ProvideCodeBase(AssemblyName = "GitHub.Exports", CodeBase = @"$PackageFolder$\GitHub.Exports.dll")]
[assembly: ProvideCodeBase(AssemblyName = "GitHub.Extensions", CodeBase = @"$PackageFolder$\GitHub.Extensions.dll")]
[assembly: ProvideCodeBase(AssemblyName = "Octokit", CodeBase = @"$PackageFolder$\Octokit.dll")]
[assembly: ProvideCodeBase(AssemblyName = "LibGit2Sharp", CodeBase = @"$PackageFolder$\LibGit2Sharp.dll")]
[assembly: ProvideCodeBase(AssemblyName = "Splat", CodeBase = @"$PackageFolder$\Splat.dll")]
[assembly: ProvideCodeBase(AssemblyName = "Rothko", CodeBase = @"$PackageFolder$\Rothko.dll")]
[assembly: ProvideCodeBase(AssemblyName = "GitHub.VisualStudio.TestAutomation.AutomationIDs",
    CodeBase = @"$PackageFolder$\GitHub.VisualStudio.TestAutomation.AutomationIDs.dll")]
