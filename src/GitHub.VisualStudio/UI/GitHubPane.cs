using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.UI
{
    using Microsoft.VisualStudio.Shell.Interop;
    using System.ComponentModel.Design;
    using Views.Controls;

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
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubPane"/> class.
        /// </summary>
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

        protected override void Initialize()
        {
            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new GitHubPaneControl();
            base.Initialize();
        }
    }
}
