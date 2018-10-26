using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model for displaying a pull request in a document window.
    /// </summary>
    [Export(typeof(IPullRequestPageViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestPageViewModel : PullRequestViewModelBase, IPullRequestPageViewModel
    {
        readonly IViewViewModelFactory factory;
        readonly IPullRequestSessionManager sessionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestPageViewModel"/> class.
        /// </summary>
        /// <param name="factory">The view model factory.</param>
        [ImportingConstructor]
        public PullRequestPageViewModel(
            IViewViewModelFactory factory,
            IPullRequestSessionManager sessionManager)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));

            this.factory = factory;
            this.sessionManager = sessionManager;
        }

        /// <inheritdoc/>
        public IIssueishCommentThreadViewModel Thread { get; private set; }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            ActorModel currentUser,
            PullRequestDetailModel model)
        {
            await base.InitializeAsync(model).ConfigureAwait(true);

            var thread = factory.CreateViewModel<IIssueishCommentThreadViewModel>();
            await thread.InitializeAsync(
                currentUser,
                model,
                true).ConfigureAwait(true);
            Thread = thread;
        }
    }
}
