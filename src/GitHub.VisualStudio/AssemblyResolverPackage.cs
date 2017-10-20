using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    // Allow assemblies in the extension directory to be resolved by their full or partial name.
    // This is required for GitHub.VisualStudio.imagemanifest, XAML and when using unsigned assemblies.
    // See: https://github.com/github/VisualStudio/pull/1236/
    [ProvideBindingPath]
    
    [Guid(Guids.guidAssemblyResolverPkgString)]
    public class AssemblyResolverPackage : Package
    {
    }
}
