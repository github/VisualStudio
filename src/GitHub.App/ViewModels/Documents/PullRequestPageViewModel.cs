using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.Documents
{
    /// <summary>
    /// View model for displaying a pull request in a document window.
    /// </summary>
    [Export(typeof(IPullRequestPageViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestPageViewModel : PullRequestViewModelBase, IPullRequestPageViewModel, IIssueishCommentThreadViewModel
    {
        readonly IViewViewModelFactory factory;
        readonly IPullRequestService service;
        readonly IPullRequestSessionManager sessionManager;
        readonly ITeamExplorerServices teServices;
        ActorModel currentUserModel;
        ReactiveList<IViewModel> timeline;

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
        public IReadOnlyList<IViewModel> Timeline => timeline;

        /// <inheritdoc/>
        public ReactiveCommand<string, Unit> ShowCommit { get; }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            IRemoteRepositoryModel repository,
            ILocalRepositoryModel localRepository,
            ActorModel currentUser,
            PullRequestDetailModel model)
        {
            await base.InitializeAsync(repository, localRepository, model).ConfigureAwait(true);

            currentUserModel = currentUser;
            CurrentUser = new ActorViewModel(currentUser);
            timeline = new ReactiveList<IViewModel>();

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
                        await AddComment(comment).ConfigureAwait(true);
                        break;
                }
            }

            if (commits.Count > 0)
            {
                timeline.Add(new CommitSummariesViewModel(commits));
            }

            await AddPlaceholder().ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task PostComment(ICommentViewModel comment)
        {
            var address = HostAddress.Create(Repository.CloneUrl);
            var result = await service.PostComment(address, Id, comment.Body).ConfigureAwait(true);
            timeline.Remove(comment);
            await AddComment(result).ConfigureAwait(true);
            await AddPlaceholder().ConfigureAwait(true);
        }

        Task ICommentThreadViewModel.DeleteComment(ICommentViewModel comment)
        {
            throw new NotImplementedException();
        }

        Task ICommentThreadViewModel.EditComment(ICommentViewModel comment)
        {
            throw new NotImplementedException();
        }

        Task IIssueishCommentThreadViewModel.CloseIssueish(ICommentViewModel comment)
        {
            throw new NotImplementedException();
        }

        async Task AddComment(CommentModel comment)
        {
            var vm = factory.CreateViewModel<IIssueishCommentViewModel>();
            await vm.InitializeAsync(this, currentUserModel, comment, null).ConfigureAwait(true);
            timeline.Add(vm);
        }

        async Task AddPlaceholder()
        {
            var placeholder = factory.CreateViewModel<IIssueishCommentViewModel>();
            await placeholder.InitializeAsync(
                this,
                currentUserModel,
                null,
                Resources.ClosePullRequest).ConfigureAwait(true);
            timeline.Add(placeholder);
        }

        async Task DoShowCommit(string oid)
        {
            await service.FetchCommit(LocalRepository, Repository, oid).ConfigureAwait(true);
            teServices.ShowCommitDetails(oid);
        }
    }
}
