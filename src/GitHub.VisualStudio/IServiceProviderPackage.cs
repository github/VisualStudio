using System;

namespace GitHub.VisualStudio
{
    public interface IServiceProviderPackage : IServiceProvider, Microsoft.VisualStudio.Shell.IAsyncServiceProvider
    {
    }
}
