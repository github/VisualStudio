using System;
using GitHub.Services.Vssdk.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitHub.VisualStudio.Commands
{
    /// <summary>
    /// Show an info message dialog when a command is executed.
    /// </summary>
    public class ShowMessageBoxCommand : VsCommand
    {
        readonly IServiceProvider serviceProvider;
        readonly string message;

        public ShowMessageBoxCommand(Guid commandSet, int commandId,
            IServiceProvider serviceProvider, string message) : base(commandSet, commandId)
        {
            this.serviceProvider = serviceProvider;
            this.message = message;
        }

        public override Task Execute()
        {
            ShowInfoMessage(message);
            return Task.CompletedTask;
        }

        void ShowInfoMessage(string infoMessage)
        {
            ErrorHandler.ThrowOnFailure(VsShellUtilities.ShowMessageBox(serviceProvider, infoMessage, null,
                OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST));
        }
    }
}
