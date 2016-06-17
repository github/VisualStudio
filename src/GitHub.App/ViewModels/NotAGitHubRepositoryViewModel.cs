using System;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.Services;
using GitHub.UI;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="NotAGitHubRepositoryViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public NotAGitHubRepositoryViewModel()
        {
            Publish = ReactiveCommand.Create();
            Publish.Subscribe(_ => OnPublish());
        }

        /// <summary>
        /// Gets the command executed when the user clicks the "Publish to GitHub" link.
        /// </summary>
        public IReactiveCommand<object> Publish { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the repository publish dialog will be opened
        /// with the private option checked.
        /// </summary>
        public bool PublishPrivate { get; set; }

        /// <summary>
        /// Called when the <see cref="Publish"/> command is executed.
        /// </summary>
        private void OnPublish()
        {
        }
    }
}