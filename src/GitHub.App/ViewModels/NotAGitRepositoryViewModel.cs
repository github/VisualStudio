using System;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.Models;
using GitHub.Services;
using GitHub.UI;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// The view model for the "Not a Git repository" view in the GitHub pane.
    /// </summary>
    [ExportViewModel(ViewType = UIViewType.NotAGitRepository)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NotAGitRepositoryViewModel : DialogViewModelBase, INotAGitRepositoryViewModel
    {
    }
}