using System.ComponentModel.Composition;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// The view model for the "Not a Git repository" view in the GitHub pane.
    /// </summary>
    [Export(typeof(INotAGitRepositoryViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NotAGitRepositoryViewModel : PanePageViewModelBase, INotAGitRepositoryViewModel
    {
    }
}