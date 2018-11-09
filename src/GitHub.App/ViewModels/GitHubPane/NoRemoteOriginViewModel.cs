using System.ComponentModel.Composition;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// The view model for the "No Origin Remote" view in the GitHub pane.
    /// </summary>
    [Export(typeof(INoRemoteOriginViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NoRemoteOriginViewModel : PanePageViewModelBase, INoRemoteOriginViewModel
    {
    }
}
