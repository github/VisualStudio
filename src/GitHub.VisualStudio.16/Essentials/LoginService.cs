using System;
using System.ComponentModel.Composition;
using GitHub.Services;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace GitHub.VisualStudio.Essentials
{
    [Export(typeof(ILoginService))]
    public class LoginService : ILoginService
    {
        readonly CompositionServices compositionServices;
        readonly IServiceProvider serviceProvider;
        readonly JoinableTaskContext joinableTaskContext;

        [ImportingConstructor]
        public LoginService(
            CompositionServices compositionServices,
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            JoinableTaskContext joinableTaskContext)
        {
            this.compositionServices = compositionServices;
            this.serviceProvider = serviceProvider;
            this.joinableTaskContext = joinableTaskContext;
        }

        public void ShowLoginDialog()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (FullExtensionUtilities.IsInstalled(serviceProvider))
            {
                ShowLoginDialogUsingFullExtension();
            }
            else
            {
                ShowLoginDialogUsingBundledExtension();
            }
        }

        void ShowLoginDialogUsingFullExtension()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (serviceProvider.GetService(typeof(DTE)) is DTE dte &&
                dte.Commands is var commands &&
                commands.Item(Guids.guidGitHubCmdSet, PkgCmdIDList.addConnectionCommand) is Command command &&
                command.IsAvailable)
            {
                commands.Raise(command.Guid, command.ID, null, null);
            }
        }

        void ShowLoginDialogUsingBundledExtension()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var exportProvider = compositionServices.GetExportProvider();
            var dialogService = exportProvider.GetExportedValue<IDialogService>();
            joinableTaskContext.Factory.Run(() => dialogService.ShowLoginDialog());
        }
    }
}
