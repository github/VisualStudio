using System;
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
    public class NotAGitHubRepositoryViewModel : PanePageViewModelBase, INotAGitHubRepositoryViewModel
    {
        ITeamExplorerServices teamExplorerServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotAGitHubRepositoryViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public NotAGitHubRepositoryViewModel(ITeamExplorerServices teamExplorerServices)
        {
            this.teamExplorerServices = teamExplorerServices;
            Publish = ReactiveCommand.Create();
            Publish.Subscribe(_ => OnPublish());
        }

        /// <summary>
        /// Gets the command executed when the user clicks the "Publish to GitHub" link.
        /// </summary>
        public IReactiveCommand<object> Publish { get; }

        /// <summary>
        /// Called when the <see cref="Publish"/> command is executed.
        /// </summary>
        private void OnPublish()
        {
            teamExplorerServices.ShowPublishSection();
        }
    }
}