using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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
        readonly IModelServiceFactory modelServiceFactory;
        IModelService modelService;
        IPullRequestSession session;
        IAccount user;
        string title;
        IReadOnlyList<IPullRequestReviewViewModel> reviews;

        [ImportingConstructor]
        public PullRequestUserReviewsViewModel(
            IPullRequestEditorService editorService,
            IPullRequestSessionManager sessionManager,
            IModelServiceFactory modelServiceFactory)
        {
            Guard.ArgumentNotNull(editorService, nameof(editorService));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(modelServiceFactory, nameof(modelServiceFactory));

            this.editorService = editorService;
            this.sessionManager = sessionManager;
            this.modelServiceFactory = modelServiceFactory;

            NavigateToPullRequest = ReactiveCommand.Create().OnExecuteCompleted(_ =>
                NavigateTo(Invariant($"{LocalRepository.Owner}/{LocalRepository.Name}/pull/{PullRequestNumber}")));
        }

        /// <inheritdoc/>
        public ILocalRepositoryModel LocalRepository { get; private set; }

        /// <inheritdoc/>
        public string RemoteRepositoryOwner { get; private set; }

        /// <inheritdoc/>
        public int PullRequestNumber { get; private set; }

        public IAccount User
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
        public ReactiveCommand<object> NavigateToPullRequest { get; }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            ILocalRepositoryModel localRepository,
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
                modelService = await modelServiceFactory.CreateAsync(connection);
                User = await modelService.GetUser(login);
                await Refresh();
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
                var pullRequest = await modelService.GetPullRequest(RemoteRepositoryOwner, LocalRepository.Name, PullRequestNumber);
                await Load(User, pullRequest);
            }
            catch (Exception ex)
            {
                log.Error(
                    ex,
                    "Error loading pull request reviews {Owner}/{Repo}/{Number} from {Address}",
                    RemoteRepositoryOwner,
                    LocalRepository.Name,
                    PullRequestNumber,
                    modelService.ApiClient.HostAddress.Title);
                Error = ex;
                IsBusy = false;
            }
        }

        /// <inheritdoc/>
        async Task Load(IAccount user, IPullRequestModel pullRequest)
        {
            IsBusy = true;

            try
            {
                session = await sessionManager.GetSession(pullRequest);
                User = user;
                PullRequestTitle = pullRequest.Title;

                var reviews = new List<IPullRequestReviewViewModel>();
                var isFirst = true;

                foreach (var review in pullRequest.Reviews.OrderByDescending(x => x.SubmittedAt))
                {
                    if (review.User.Login == user.Login &&
                        review.State != PullRequestReviewState.Pending)
                    {
                        var vm = new PullRequestReviewViewModel(editorService, session, pullRequest, review);
                        vm.IsExpanded = isFirst;
                        reviews.Add(vm);
                        isFirst = false;
                    }
                }

                Reviews = reviews;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}