using System;
using System.Runtime.InteropServices;
using GitHub.VisualStudio.UI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// This is the host package for the <see cref="GitHubPane"/> tool window.
    /// </summary>
    /// <remarks>
    /// We auto-load this package before its tool window is activated to ensure that MEF has
    /// been initialized and the tool window won't be blamed for degrading startup performance.
    /// See: https://github.com/github/VisualStudio/issues/1550
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.InitializeMefPackageId)]
    // This package must auto-load before its tool window is activated on startup.
    // We're using the GitSccProvider UI context because this is triggered before
    // tool windows are activated and we already use it elsewhere (the containing
    // assembly would have been loaded anyway).
    [ProvideAutoLoad(Guids.GitSccProviderId)]
    [ProvideToolWindow(typeof(GitHubPane), Orientation = ToolWindowOrientation.Right,
        Style = VsDockStyle.Tabbed, Window = EnvDTE.Constants.vsWindowKindSolutionExplorer)]
    public sealed class GitHubPanePackage : Package
    {
        /// <summary>
        /// Ensure that MEF is initialized using an old style non-async package.
        /// This causes the `Scanning new and updated MEF components...` dialog to appear.
        /// Without this the GitHub pane will be blamed for degrading startup performance.
        /// Initialize must be called before its tool window is activated (otherwise the
        /// tool window still end up waiting for it).
        /// </summary>
        protected override void Initialize()
        {
            GetService(typeof(SComponentModel));
        }
    }
}
