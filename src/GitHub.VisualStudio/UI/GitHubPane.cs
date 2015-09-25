using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using GitHub.Services;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using System.Diagnostics;
using GitHub.Extensions;
using GitHub.VisualStudio.UI.Views.Controls;
using GitHub.Exports;
using GitHub.Models;
using GitHub.UI;
using System.Linq;
using GitHub.Primitives;

namespace GitHub.VisualStudio.UI
{

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("6b0fdc0a-f28e-47a0-8eed-cc296beff6d2")]
    public class GitHubPane : ToolWindowPane
    {
        IDisposable disposable;

        public GitHubPane() : base(null)
        {
            Caption = "GitHub";

            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;
            ToolBar = new CommandID(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.idGitHubToolbar);
            ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

        }

        protected override void Initialize()
        {
            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new GitHubPaneControl();
            this.GetExportedValue<IUIProvider>().GetService<ITeamExplorerServiceHolder>().ServiceProvider = this;

            AddTopLevelMenuItem(GuidList.guidGitHubToolbarCmdSet, PkgCmdIDList.pullRequestCommand, (s, e) => MoveToPage(UIControllerFlow.PullRequestList));

            base.Initialize();
        }

        void MoveToPage(UIControllerFlow type)
        {
            var uiProvider = this.GetExportedValue<IUIProvider>();
            var factory = uiProvider.GetService<IExportFactoryProvider>();
            var uiflow = factory.UIControllerFactory.CreateExport();
            disposable = uiflow;
            var ui = uiflow.Value;
            var creation = ui.SelectFlow(type);
            creation.Subscribe(c =>
            {
                ((GitHubPaneControl)Content).container.Children.Clear();
                ((GitHubPaneControl)Content).container.Children.Add(c);
            });
            var activeRepo = uiProvider.GetService<ITeamExplorerServiceHolder>().ActiveRepo;
            if (activeRepo != null)
            {
                var cm = uiProvider.GetService<IConnectionManager>();
                var conn = cm.Connections.FirstOrDefault(c => c.HostAddress.Equals(HostAddress.Create(activeRepo.CloneUrl)));
                ui.Start(conn);
            }
            else
                ui.Start(null);
        }

        void AddTopLevelMenuItem(
            Guid guid,
            int cmdId,
            EventHandler eventHandler)
        {
            var mcs = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            Debug.Assert(mcs != null, "No IMenuCommandService? Something is wonky");
            if (mcs == null)
                return;
            var id = new CommandID(guid, cmdId);
            var item = new MenuCommand(eventHandler, id);
            mcs.AddCommand(item);
        }

        void AddDynamicMenuItem(
            Guid guid,
            int cmdId,
            Func<bool> canEnable,
            Action execute)
        {
            var mcs = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            Debug.Assert(mcs != null, "No IMenuCommandService? Something is wonky");
            if (mcs == null)
                return;
            var id = new CommandID(guid, cmdId);
            var item = new OleMenuCommand(
                (s, e) => execute(),
                (s, e) => { },
                (s, e) =>
                {
                    ((OleMenuCommand)s).Visible = canEnable();
                },
                id);
            mcs.AddCommand(item);
        }

        bool disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    if (disposable != null)
                        disposable.Dispose();
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }
    }
}
