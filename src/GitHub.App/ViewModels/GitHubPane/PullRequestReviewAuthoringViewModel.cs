using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IPullRequestReviewAuthoringViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestReviewAuthoringViewModel : PanePageViewModelBase, IPullRequestReviewAuthoringViewModel
    {
        static readonly ILogger log = LogManager.ForContext<PullRequestReviewAuthoringViewModel>();

        readonly IPullRequestEditorService editorService;
        readonly IPullRequestSessionManager sessionManager;
        readonly IModelServiceFactory modelServiceFactory;
        IModelService modelService;
        IPullRequestSession session;
        IDisposable sessionSubscription;
        IPullRequestReviewModel model;
        IPullRequestModel pullRequestModel;
        string body;
        ObservableAsPropertyHelper<bool> canApproveRequestChanges;
        IReadOnlyList<IPullRequestReviewFileCommentViewModel> fileComments;
        string operationError;

        [ImportingConstructor]
        public PullRequestReviewAuthoringViewModel(
            IPullRequestEditorService editorService,
            IPullRequestSessionManager sessionManager,
            IModelServiceFactory modelServiceFactory,
            IPullRequestFilesViewModel files)
        {
            Guard.ArgumentNotNull(editorService, nameof(editorService));
            Guard.ArgumentNotNull(sessionManager, nameof(sessionManager));
            Guard.ArgumentNotNull(modelServiceFactory, nameof(modelServiceFactory));
            Guard.ArgumentNotNull(files, nameof(files));

            this.editorService = editorService;
            this.sessionManager = sessionManager;
            this.modelServiceFactory = modelServiceFactory;

            canApproveRequestChanges = this.WhenAnyValue(
                x => x.Model,
                x => x.PullRequestModel,
                (review, pr) => review != null && pr != null && review.User.Login != pr.Author.Login)
                .ToProperty(this, x => x.CanApproveRequestChanges);

            Files = files;
            Submit = ReactiveCommand.CreateAsyncTask(DoSubmit);
        }

        /// <inheritdoc/>
        public ILocalRepositoryModel LocalRepository { get; private set; }

        /// <inheritdoc/>
        public string RemoteRepositoryOwner { get; private set; }

        /// <inheritdoc/>
        public IPullRequestReviewModel Model
        {
            get { return model; }
            private set { this.RaiseAndSetIfChanged(ref model, value); }
        }

        /// <inheritdoc/>
        public IPullRequestModel PullRequestModel
        {
            get { return pullRequestModel; }
            private set { this.RaiseAndSetIfChanged(ref pullRequestModel, value); }
        }

        /// <inheritdoc/>
        public IPullRequestFilesViewModel Files { get; }

        /// <inheritdoc/>
        public string Body
        {
            get { return body; }
            set { this.RaiseAndSetIfChanged(ref body, value); }
        }

        /// <inheritdoc/>
        public bool CanApproveRequestChanges => canApproveRequestChanges.Value;

        /// <summary>
        /// Gets the error message to be displayed in the action area as a result of an error in a
        /// git operation.
        /// </summary>
        public string OperationError
        {
            get { return operationError; }
            private set { this.RaiseAndSetIfChanged(ref operationError, value); }
        }

        /// <inheritdoc/>
        public IReadOnlyList<IPullRequestReviewFileCommentViewModel> FileComments
        {
            get { return fileComments; }
            private set { this.RaiseAndSetIfChanged(ref fileComments, value); }
        }

        public ReactiveCommand<object> NavigateToPullRequest { get; }
        public ReactiveCommand<Unit> Submit { get; }

        public async Task InitializeAsync(
            ILocalRepositoryModel localRepository,
            IConnection connection,
            string owner,
            string repo,
            int pullRequestNumber)
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
                modelService = await modelServiceFactory.CreateAsync(connection);
                var pullRequest = await modelService.GetPullRequest(
                    RemoteRepositoryOwner,
                    LocalRepository.Name,
                    pullRequestNumber);
                await Load(pullRequest);
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
                var pullRequest = await modelService.GetPullRequest(
                    RemoteRepositoryOwner,
                    LocalRepository.Name,
                    PullRequestModel.Number);
                await Load(pullRequest);
            }
            catch (Exception ex)
            {
                log.Error(
                    ex,
                    "Error loading pull request review {Owner}/{Repo}/{Number}/{PullRequestReviewId} from {Address}",
                    RemoteRepositoryOwner,
                    LocalRepository.Name,
                    PullRequestModel.Number,
                    Model.Id,
                    modelService.ApiClient.HostAddress.Title);
                Error = ex;
                IsBusy = false;
            }
        }

        async Task Load(IPullRequestModel pullRequest)
        {
            try
            {
                session = await sessionManager.GetSession(pullRequest);
                PullRequestModel = pullRequest;

                Model = pullRequest.Reviews.FirstOrDefault(x =>
                    x.State == PullRequestReviewState.Pending && x.User.Login == session.User.Login) ??
                    new PullRequestReviewModel
                    {
                        Body = string.Empty,
                        User = session.User,
                        State = PullRequestReviewState.Pending,
                    };

                Body = Model.Body;

                sessionSubscription?.Dispose();
                await UpdateFileComments();
                sessionSubscription = session.PullRequestChanged.Subscribe(_ => UpdateFileComments().Forget());
            }
            finally
            {
                IsBusy = false;
            }
        }

        bool FilterComments(IInlineCommentThreadModel thread)
        {
            return thread.Comments.Any(x => x.PullRequestReviewId == Model.Id);
        }

        async Task UpdateFileComments()
        {
            var result = new List<PullRequestReviewFileCommentViewModel>();

            //if (Model.Id == 0 && session.PendingReviewId != 0)
            //{
            //    ((PullRequestReviewModel)Model).Id = session.PendingReviewId;
            //}

            foreach (var file in await session.GetAllFiles())
            {
                foreach (var thread in file.InlineCommentThreads)
                {
                    foreach (var comment in thread.Comments)
                    {
                        if (comment.PullRequestReviewId == Model.Id)
                        {
                            result.Add(new PullRequestReviewFileCommentViewModel(
                                editorService,
                                session,
                                comment));
                        }
                    }
                }
            }

            FileComments = result;
            await Files.InitializeAsync(session, FilterComments);
        }

        async Task DoSubmit(object arg)
        {
            OperationError = null;
            IsBusy = true;

            try
            {
                Octokit.PullRequestReviewEvent e;

                if (Enum.TryParse(arg.ToString(), out e))
                {
                    await Task.Delay(1);
                    //await session.PostReview(Body, e);
                    Close();
                }
            }
            catch (Exception ex)
            {
                OperationError = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
