using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.SampleData;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model for displaying a pull request in a document window.
    /// </summary>
    [Export(typeof(IPullRequestPageViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestPageViewModel : PullRequestViewModelBase, IPullRequestPageViewModel, ICommentThreadViewModel
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
        public IActorViewModel CurrentUser { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<IViewModel> Timeline { get; private set; }

        /// <inheritdoc/>
        IReadOnlyReactiveList<ICommentViewModel> ICommentThreadViewModel.Comments => throw new NotImplementedException();

        /// <inheritdoc/>
        public async Task InitializeAsync(
            ActorModel currentUser,
            PullRequestDetailModel model)
        {
            await base.InitializeAsync(model).ConfigureAwait(true);

            CurrentUser = new ActorViewModel(currentUser);

            var timeline = new ReactiveList<IViewModel>();
            var commits = new List<CommitSummaryViewModel>();

            foreach (var i in model.Timeline)
            {
                if (!(i is CommitModel) && commits.Count > 0)
                {
                    timeline.Add(new CommitSummariesViewModel(commits));
                    commits.Clear();
                }

                switch (i)
                {
                    case CommitModel commit:
                        commits.Add(new CommitSummaryViewModel(commit));
                        break;
                    case CommentModel comment:
                        var vm = factory.CreateViewModel<ICommentViewModel>();
                        await vm.InitializeAsync(this, currentUser, comment, CommentEditState.None).ConfigureAwait(true);
                        timeline.Add(vm);
                        break;
                }
            }

            if (commits.Count > 0)
            {
                timeline.Add(new CommitSummariesViewModel(commits));
            }

            Timeline = timeline;
        }

        Task ICommentThreadViewModel.DeleteComment(int pullRequestId, int commentId)
        {
            throw new NotImplementedException();
        }

        Task ICommentThreadViewModel.EditComment(string id, string body)
        {
            throw new NotImplementedException();
        }

        Task ICommentThreadViewModel.PostComment(string body)
        {
            throw new NotImplementedException();
        }
    }
}
