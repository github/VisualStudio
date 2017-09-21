using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    [ProvideBindingPath]
    [Guid(Guids.guidAssemblyResolverPkgString)]
    public class AssemblyResolverPackage : Package
    {
    }
}
