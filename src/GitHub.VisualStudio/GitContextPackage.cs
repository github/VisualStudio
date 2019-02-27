using System;
using System.Threading;
using System.Runtime.InteropServices;
using GitHub.Exports;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using static Microsoft.VisualStudio.VSConstants;
using Microsoft;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// This package creates a custom UIContext <see cref="Guids.GitContextPkgString"/> that is activated when a
    /// repository is active in <see cref="IVSGitExt"/> and the current process is Visual Studio (not Blend).
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(Guids.GitContextPkgString)]
    // Initialize when we enter the context of a Git repository
    [ProvideAutoLoad(UICONTEXT.RepositoryOpen_string, PackageAutoLoadFlags.BackgroundLoad)]
    public class GitContextPackage : AsyncPackage
    {
        protected async override Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            if (!ExportForVisualStudioProcessAttribute.IsVisualStudioProcess())
            {
                // Don't activate 'GitContextPkgString' for non-Visual Studio process
                return;
            }

            var gitExt = (IVSGitExt)await GetServiceAsync(typeof(IVSGitExt));
            Assumes.Present(gitExt);

            var context = UIContext.FromUIContextGuid(new Guid(Guids.GitContextPkgString));
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