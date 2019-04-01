using System;
using System.Windows;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using GitHub.Services;
using GitHub.Factories;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;
using DTE = EnvDTE.DTE;
using Task = System.Threading.Tasks.Task;

namespace GitHubCore
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenFromGitHubCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("aa0f1730-6a63-4ae3-9b61-3a1f34761663");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFromGitHubCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenFromGitHubCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenFromGitHubCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get => package;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in OpenFromGitHubCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenFromGitHubCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            Assumes.Present(dte);
            try
            {
                dte.ExecuteCommand("GitHub.OpenFromUrl");
            }
            catch (COMException)
            {
                ShowCloneDialogAsync(ServiceProvider).FileAndForget("ShowCloneDialogAsync");
            }
        }

        [STAThread]
        static async Task ShowCloneDialogAsync(IServiceProvider serviceProvider)
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            Assumes.Present(componentModel);
            var compositionServices = new CompositionServices();
            var compositionContainer = compositionServices.CreateCompositionContainer(componentModel.DefaultExportProvider);

            var url = null as string;
            var dialogService = compositionContainer.GetExportedValue<IDialogService>();
            var cloneDialogResult = await dialogService.ShowCloneDialog(null, url);
            if (cloneDialogResult != null)
            {
                var repositoryCloneService = compositionContainer.GetExportedValue<IRepositoryCloneService>();
                await repositoryCloneService.CloneOrOpenRepository(cloneDialogResult);
            }
        }
    }
}
