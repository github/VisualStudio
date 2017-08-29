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
using GitHub.ViewModels;
using System.Diagnostics;
using GitHub.VisualStudio.UI.Views;

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
    [Guid(GitHubPaneGuid)]
    public class GitHubPane : ToolWindowPane, IServiceProviderAware, IViewHost
    {
        public const string GitHubPaneGuid = "6b0fdc0a-f28e-47a0-8eed-cc296beff6d2";
        bool initialized = false;

        IView View
        {
            get { return Content as IView; }
            set { Content = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GitHubPane() : base(null)
        {
            Caption = "GitHub";

            BitmapImageMoniker = new Microsoft.VisualStudio.Imaging.Interop.ImageMoniker()
            {
                Guid = Guids.guidImageMoniker,
                Id = 1
            };
            ToolBar = new CommandID(Guids.guidGitHubToolbarCmdSet, PkgCmdIDList.idGitHubToolbar);
            ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
            var provider = Services.GitHubServiceProvider;
            var uiProvider = provider.GetServiceSafe<IUIProvider>();
            View = uiProvider.GetView(Exports.UIViewType.GitHubPane);
        }

        protected override void Initialize()
        {
            base.Initialize();
            Initialize(this);
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            if (!initialized)
            {
                initialized = true;

                var vm = View.ViewModel as IServiceProviderAware;
                Debug.Assert(vm != null);
                vm?.Initialize(serviceProvider);
            }
        }

        public void ShowView(ViewWithData data)
        {
            View.ViewModel?.Initialize(data);
        }
    }
}
