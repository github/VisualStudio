using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;
using Serilog;
using static System.FormattableString;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Displays all reviews made by a user on a pull request.
    /// </summary>
    [Export(typeof(IPullRequestUserReviewsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestUserReviewsViewModel : PanePageViewModelBase, IPullRequestUserReviewsViewModel
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestReviewViewModel>();

        readonly IPullRequestEditorService editorService;
        readonly IPullRequestSessionManager sessionManager;
        IPullRequestSession session;
        string login;
        IActorViewModel user;
        string title;
        IReadOnlyList<IPullRequestReviewViewModel> reviews;

        [ImportingConstructor]
        public PullRequestUserReviewsViewModel(
            IPullRequestEditorService editorService,
            IPullRequestSessionManager sessionManager)
        {
            Guard.ArgumentNotNull(editorService, nameof(editorService));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));

            this.editorService = editorService;
            this.sessionManager = sessionManager;

            NavigateToPullRequest = ReactiveCommand.Create(() =>
                NavigateTo(Invariant($"{LocalRepository.Owner}/{LocalRepository.Name}/pull/{PullRequestNumber}")));
        }

        /// <inheritdoc/>
        public LocalRepositoryModel LocalRepository { get; private set; }

        /// <inheritdoc/>
        public string RemoteRepositoryOwner { get; private set; }

        /// <inheritdoc/>
        public int PullRequestNumber { get; private set; }

        public IActorViewModel User
        {
            get { return user; }
            private set { this.RaiseAndSetIfChanged(ref user, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestReviewViewModel> Reviews
        {
            get { return reviews; }
            private set { this.RaiseAndSetIfChanged(ref reviews, value); }
        }

        /// <inheritdoc/>
        public string PullRequestTitle
        {
            get { return title; }
            private set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        /// <inheritdoc/>
        public ReactiveCommand<Unit, Unit> NavigateToPullRequest { get; }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "login")]
        public async Task InitializeAsync(
            LocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber,
            string login)
        {
            if (repo != localRepository.Name)
            {
                throw new NotSupportedException();
            }

            IsLoading = true;

            try
            {
                LocalRepository = localRepository;
                RemoteRepositoryOwner = owner;
                PullRequestNumber = pullRequestNumber;
                this.login = login;
                session = await sessionManager.GetSession(owner, repo, pullRequestNumber);
                await Load(session.PullRequest);                
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <inheritdoc/>
        public override async Task Refresh()
        {
            try
            {
                Error = null;
                IsBusy = true;
                await session.Refresh();
                await Load(session.PullRequest);
            }
            catch (Exception ex)
            {
                log.Error(
                    ex,
                    "Error loading pull request reviews {Owner}/{Repo}/{Number} from {Address}",
                    RemoteRepositoryOwner,
                    LocalRepository.Name,
                    PullRequestNumber,
                    LocalRepository.CloneUrl.Host);
                Error = ex;
                IsBusy = false;
            }
        }

        /// <inheritdoc/>
        async Task Load(PullRequestDetailModel pullRequest)
        {
            IsBusy = true;

            try
            {
                await Task.Delay(0);
                PullRequestTitle = pullRequest.Title;

                var reviews = new List<IPullRequestReviewViewModel>();
                var isFirst = true;

                foreach (var review in pullRequest.Reviews.OrderByDescending(x => x.SubmittedAt))
                {
                    if (review.Author.Login == login)
                    {
                        if (User == null)
                        {
                            User = new ActorViewModel(review.Author);
                        }

                        if (review.State != PullRequestReviewState.Pending)
                        {
                            var vm = new PullRequestReviewViewModel(editorService, session, review);
                            vm.IsExpanded = isFirst;
                            reviews.Add(vm);
                            isFirst = false;
                        }
                    }
                }

                Reviews = reviews;

                if (User == null)
                {
                    User = new ActorViewModel(new ActorModel { Login = login });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}