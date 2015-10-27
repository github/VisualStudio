using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using System.Windows.Controls;
using GitHub.Services;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.ViewModels;
using System.Diagnostics;

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
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

            var factory = this.GetExportedValue<IUIProvider>().GetService<IExportFactoryProvider>();
            // placeholder logic to load the view until the UIController is able to do it for us
            Content = factory.GetView(Exports.UIViewType.GitHubPane).Value;
            (Content as UserControl).DataContext = factory.GetViewModel(Exports.UIViewType.GitHubPane).Value;
        }

        protected override void Initialize()
        {
            base.Initialize();
            var vm = (Content as IView).ViewModel as IServiceProviderAware;
            Debug.Assert(vm != null);
            vm?.Initialize(this);
        }
    }
}
