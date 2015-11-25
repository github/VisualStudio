using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using GitHub.VisualStudio.Base;
using GitHub.VisualStudio.UI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Octokit;
using IConnection = GitHub.Models.IConnection;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidGitHubPkgString)]
    //[ProvideBindingPath]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    //[ProvideAutoLoad(UIContextGuids.NoSolution)]
    [ProvideAutoLoad("11B8E6D7-C08B-4385-B321-321078CDD1F8")]
    [ProvideToolWindow(typeof(GitHubPane), Orientation = ToolWindowOrientation.Right, Style = VsDockStyle.Tabbed, Window = EnvDTE.Constants.vsWindowKindSolutionExplorer)]
    public class GitHubPackage : PackageBase
    {
        public GitHubPackage()
        {
        }

        public GitHubPackage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {

        }

        protected override void Initialize()
        {
            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubCmdSet, PkgCmdIDList.addConnectionCommand, (s, e) => StartFlow(UIControllerFlow.Authentication));
            ServiceProvider.AddTopLevelMenuItem(GuidList.guidCreateGistCommandPackageCmdSet, PkgCmdIDList.createGistCommand,
                (s, e) =>
                {
                    // All this code should get moved somewhere else, not sure where though, any pointers?!
                    IsUserLoggedIn().ContinueWith(async isLoggedIn =>
                    {
                        // Log in the user if they are not already logged in.
                        if (!isLoggedIn.Result)
                        {
                            // Can we use the Authentication UIControllerFlow here and wait for a successful authentication before proceeding?
                        }

                        var highlightedText = GetHighlightedText();

                        // TODO Show a popup with 
                        //  - TextBox to name the gist
                        //  - TextBox to give the gist a description
                        //  - Checkbox for public gist
                        //  - Not sure about this one, but may be nice to open the newly created gist in GitHub checkbox
                        //  - Not sure if we really need to re-display the selected text as it should still be selected in the text editor.

                        // It may be useful to return the created gist if we support an "Open Gist in GitHub" checkbox feature 
                        // that will auto open the newly created Gist if checked.
                        var createdGist = await CreateGist("NameWillBeEnteredInThePopup",
                            "DescriptionWillBeEnteredInThePopup", true, highlightedText);
                    });
                });

            ServiceProvider.AddTopLevelMenuItem(GuidList.guidGitHubCmdSet, PkgCmdIDList.showGitHubPaneCommand, (s, e) =>
            {
                var window = FindToolWindow(typeof(GitHubPane), 0, true);
                if (window?.Frame == null)
                    throw new NotSupportedException("Cannot create tool window");

                var windowFrame = (IVsWindowFrame)window.Frame;
                ErrorHandler.ThrowOnFailure(windowFrame.Show());
            });
            base.Initialize();
        }

        void StartFlow(UIControllerFlow controllerFlow)
        {
            var uiProvider = ServiceProvider.GetExportedValue<IUIProvider>();
            uiProvider.RunUI(controllerFlow, null);
        }

        string GetHighlightedText()
        {
            var textManager = (IVsTextManager)ServiceProvider.GetService(typeof(SVsTextManager));
            if (textManager == null)
                return string.Empty;

            IVsTextView activeView;
            if (textManager.GetActiveView(1, null, out activeView) != VSConstants.S_OK)
                return string.Empty;

            string highlightedText;
            if (activeView.GetSelectedText(out highlightedText) != VSConstants.S_OK)
                return string.Empty;

            return highlightedText;
        }

        async Task<bool> IsUserLoggedIn()
        {
            var repoHosts = ServiceProvider.GetExportedValue<IRepositoryHosts>();
            var connMgr = ServiceProvider.GetExportedValue<IConnectionManager>();

            return await connMgr.IsLoggedIn(repoHosts);
        }

        async Task<Gist> CreateGist(string name, string description, bool isPublic, string content)
        {
            Guard.ArgumentNotEmptyString(name, nameof(name));

            var newGist = new NewGist
            {
                Description = description,
                Public = isPublic
            };
            newGist.Files.Add(name, content);

            var githubClient = ServiceProvider.GetExportedValue<IGitHubClient>();
            return await githubClient.Gist.Create(newGist);
        }
    }
}
