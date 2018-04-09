using System;
using System.Runtime.InteropServices;
using GitHub.VisualStudio.UI;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio
{
    /// <summary>
    /// This is the host package for the <see cref="GitHubPane"/> tool window.
    /// </summary>
    /// <remarks>
    /// This package mustn't use MEF.
    /// See: https://github.com/github/VisualStudio/issues/1550
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(Guids.GitHubPanePackageId)]
    [ProvideToolWindow(typeof(GitHubPane), Orientation = ToolWindowOrientation.Right,
        Style = VsDockStyle.Tabbed, Window = EnvDTE.Constants.vsWindowKindSolutionExplorer)]
    public sealed class GitHubPanePackage : AsyncPackage
    {
    }
}