using System;
using System.Threading;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitHub.Services.Vssdk
{
    public abstract class AsyncMenuPackage : AsyncPackage
    {
        IVsUIShell vsUIShell;

        sealed protected async override Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            vsUIShell = await GetServiceAsync(typeof(SVsUIShell)) as IVsUIShell;

            var menuCommandService = (OleMenuCommandService)(await GetServiceAsync(typeof(IMenuCommandService)));
            await InitializeMenusAsync(menuCommandService);
        }

        protected abstract Task InitializeMenusAsync(OleMenuCommandService menuService);

        // The IDesignerHost, ISelectionService and IVsUIShell are requested by the MenuCommandService.
        // This override allows IMenuCommandService.AddCommands to be called form a background thread.
        protected override object GetService(Type serviceType)
        {
            if (serviceType == typeof(SVsUIShell))
            {
                return vsUIShell;
            }

            if (serviceType == typeof(ISelectionService) || serviceType == typeof(IDesignerHost))
            {
                return null;
            }

            return base.GetService(serviceType);
        }
    }
}
