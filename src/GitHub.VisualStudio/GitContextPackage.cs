using System;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using GitHub.Services;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// This package creates a custom UIContext <see cref="Guids.UIContext_Git"/> that is activated when a
    /// repository is active in <see cref="IVSGitExt"/>.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(Guids.UIContext_Git)]
    // this is the Git service GUID, so we load whenever it loads
    [ProvideAutoLoad(Guids.GitSccProviderId, PackageAutoLoadFlags.BackgroundLoad)]
    public class GitContextPackage : AsyncPackage
    {
        protected async override Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var gitExt = (IVSGitExt)await GetServiceAsync(typeof(IVSGitExt));
            var context = UIContext.FromUIContextGuid(new Guid(Guids.UIContext_Git));
            RefreshContext(context, gitExt);
            gitExt.ActiveRepositoriesChanged += () =>
            {
                RefreshContext(context, gitExt);
            };
        }

        static void RefreshContext(UIContext context, IVSGitExt gitExt)
        {
            context.IsActive = gitExt.ActiveRepositories.Count > 0;
        }
    }
}