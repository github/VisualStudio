using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive;
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
        readonly IPullRequestService service;
        readonly IPullRequestSessionManager sessionManager;
        readonly ITeamExplorerServices teServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestPageViewModel"/> class.
        /// </summary>
        /// <param name="factory">The view model factory.</param>
        [ImportingConstructor]
        public PullRequestPageViewModel(
            IViewViewModelFactory factory,
            IPullRequestService service,
            IPullRequestSessionManager sessionManager,
            ITeamExplorerServices teServices)
        {
            Guard.ArgumentNotNull(factory, nameof(factory));
            Guard.ArgumentNotNull(service, nameof(service));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(teServices, nameof(teServices));

            this.factory = factory;
            this.service = service;
            this.sessionManager = sessionManager;
            this.teServices = teServices;

            ShowCommit = ReactiveCommand.CreateFromTask<string>(DoShowCommit);
        }

        /// <inheritdoc/>
        public IActorViewModel CurrentUser { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyList<IViewModel> Timeline { get; private set; }

        /// <inheritdoc/>
        public ReactiveCommand<string, Unit> ShowCommit { get; }

        /// <inheritdoc/>
        IReadOnlyReactiveList<ICommentViewModel> ICommentThreadViewModel.Comments => throw new NotImplementedException();

        /// <inheritdoc/>
        public async Task InitializeAsync(
            IRemoteRepositoryModel repository,
            ILocalRepositoryModel localRepository,
            ActorModel currentUser,
            PullRequestDetailModel model)
        {
            await base.InitializeAsync(repository, localRepository, model).ConfigureAwait(true);

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
                        {
                            var vm = factory.CreateViewModel<IIssueishCommentViewModel>();
                            await vm.InitializeAsync(this, currentUser, comment, null).ConfigureAwait(true);
                            timeline.Add(vm);
                        }
                        break;
                }
            }

            if (commits.Count > 0)
            {
                timeline.Add(new CommitSummariesViewModel(commits));
            }

            var placeholder = factory.CreateViewModel<IIssueishCommentViewModel>();
            await placeholder.InitializeAsync(
                this,
                currentUser,
                null,
                Resources.ClosePullRequest).ConfigureAwait(true);
            timeline.Add(placeholder);

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

        async Task DoShowCommit(string oid)
        {
            await service.FetchCommit(LocalRepository, Repository, oid).ConfigureAwait(true);
            teServices.ShowCommitDetails(oid);
        }
    }
}
