using System.ComponentModel.Composition;
using GitHub.Exports;

namespace GitHub.ViewModels
{
    /// <summary>
    /// The view model for the "Not a Git repository" view in the GitHub pane.
    /// </summary>
    [ExportViewModel(ViewType = UIViewType.NotAGitRepository)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NotAGitRepositoryViewModel : PanePageViewModelBase, INotAGitRepositoryViewModel
    {
    }
}