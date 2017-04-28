using GitHub.ViewModels;
using System;
using System.Runtime.InteropServices;

namespace GitHub.VisualStudio
{
    public interface IServiceProviderPackage : IServiceProvider, Microsoft.VisualStudio.Shell.IAsyncServiceProvider
    {
    }

    [Guid("FC9EC5B5-C297-4548-A229-F8E16365543C")]
    [ComVisible(true)]
    public interface IGitHubToolWindowManager
    {
        IViewHost ShowHomePane();
    }
}
