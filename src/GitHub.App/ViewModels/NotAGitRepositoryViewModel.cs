using System;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using NullGuard;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// The view model for the "Not a Git repository" view in the GitHub pane.
    /// </summary>
    [Export(typeof(INotAGitRepositoryViewModel))]
    [ExportViewModel(ViewType = UIViewType.NotAGitRepository)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [NullGuard(ValidationFlags.None)]
    public class NotAGitRepositoryViewModel : BaseViewModel, INotAGitRepositoryViewModel
    {
        ReactiveCommand<object> IGitHubPanePage.Refresh => null;
    }
}