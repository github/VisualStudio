using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using GitHub.App;
using GitHub.Factories;
using GitHub.Logging;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;
using Serilog;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// View model for displaying details of a pull request review.
    /// </summary>
    [Export(typeof(IPullRequestReviewViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestReviewViewModel : PanePageViewModelBase, IPullRequestReviewViewModel
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestReviewViewModel>();

        readonly IPullRequestService pullRequestsService;
        readonly IPullRequestSessionManager sessionManager;
        readonly IModelServiceFactory modelServiceFactory;
        IModelService modelService;
        IPullRequestReviewModel model;
        string state;
        string body;
        IPullRequestFilesViewModel files;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestReviewViewModel"/> class.
        /// </summary>
        /// <param name="pullRequestsService">The pull requests service.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="modelServiceFactory">The model service factory.</param>
        [ImportingConstructor]
        public PullRequestReviewViewModel(
            IPullRequestService pullRequestsService,
            IPullRequestSessionManager sessionManager,
            IModelServiceFactory modelServiceFactory)
        {
            this.pullRequestsService = pullRequestsService;
            this.sessionManager = sessionManager;
            this.modelServiceFactory = modelServiceFactory;
        }

        /// <inheritdoc/>
        public ILocalRepositoryModel LocalRepository { get; private set; }

        /// <inheritdoc/>
        public string RemoteRepositoryOwner { get; private set; }

        /// <inheritdoc/>
        public int PullRequestNumber { get; private set; }

        /// <inheritdoc/>
        public long PullRequestReviewId { get; private set; }

        /// <inheritdoc/>
        public IPullRequestFilesViewModel Files
        {
            get { return files; }
            private set { this.RaiseAndSetIfChanged(ref files, value); }
        }

        /// <inheritdoc/>
        public IPullRequestReviewModel Model
        {
            get { return model; }
            private set { this.RaiseAndSetIfChanged(ref model, value); }
        }

        /// <inheritdoc/>
        public string State
        {
            get { return state; }
            private set { this.RaiseAndSetIfChanged(ref state, value); }
        }

        /// <inheritdoc/>
        public string Body
        {
            get { return body; }
            private set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(
            ILocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber,
            long pullRequestReviewId)
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
                PullRequestReviewId = pullRequestReviewId;
                modelService = await modelServiceFactory.CreateAsync(connection);
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
                await Load(pullRequest);
            }
            catch (Exception ex)
            {
                log.Error(
                    ex,
                    "Error loading pull request review {Owner}/{Repo}/{Number}/{PullRequestReviewId} from {Address}",
                    RemoteRepositoryOwner,
                    LocalRepository.Name,
                    PullRequestNumber,
                    PullRequestReviewId,
                    modelService.ApiClient.HostAddress.Title);
                Error = ex;
                IsBusy = false;
            }
        }

        /// <inheritdoc/>
        public async Task Load(IPullRequestModel pullRequest)
        {
            try
            {
                Model = pullRequest.Reviews.Single(x => x.Id == PullRequestReviewId);
                State = PullRequestDetailReviewItem.ToString(Model.State);
                Body = !string.IsNullOrWhiteSpace(Model.Body) ? Model.Body : Resources.NoDescriptionProvidedMarkdown;

                var session = await sessionManager.GetSession(pullRequest);
                var changes = await pullRequestsService.GetTreeChanges(LocalRepository, pullRequest);
                Files = new PullRequestFilesViewModel();
                await Files.InitializeAsync(LocalRepository.LocalPath, session, changes, FilterComments);
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Files?.Dispose();
                Files = null;
            }
        }

        bool FilterComments(IInlineCommentThreadModel thread)
        {
            return thread.Comments.Any(x => x.PullRequestReviewId == PullRequestReviewId);
        }
    }
}
