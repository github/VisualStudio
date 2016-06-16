using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// The view model for the "Not a GitHub repository" view in the GitHub pane.
    /// </summary>
    [ExportViewModel(ViewType = UIViewType.NotAGitHubRepository)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class NotAGitHubRepositoryViewModel : BaseViewModel, INotAGitHubRepositoryViewModel
    {
        IUIProvider uiProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotAGitHubRepositoryViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public NotAGitHubRepositoryViewModel(IUIProvider uiProvider)
        {
            this.uiProvider = uiProvider;
            Publish = ReactiveCommand.Create();
        }

        /// <summary>
        /// Gets the command executed when the user clicks the "Publish to GitHub" link.
        /// </summary>
        public IReactiveCommand<object> Publish { get; }
    }
}