using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Media.Imaging;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <inheritdoc cref="IPullRequestCheckViewModel"/>
    [Export(typeof(IPullRequestCheckViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestCheckViewModel: ViewModelBase, IPullRequestCheckViewModel
    {
        const string DefaultAvatar = "pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png";

        private readonly IUsageTracker usageTracker;

        /// <summary>
        /// Factory method to create a <see cref="PullRequestCheckViewModel"/>.
        /// </summary>
        /// <param name="viewViewModelFactory">A viewviewmodel factory.</param>
        /// <param name="pullRequest">The pull request.</param>
        public static IEnumerable<IPullRequestCheckViewModel> Build(IViewViewModelFactory viewViewModelFactory, PullRequestDetailModel pullRequest)
        {
            var statuses = pullRequest.Statuses?.Select(statusModel =>
            {
                PullRequestCheckStatus checkStatus;
                switch (statusModel.State)
                {
                    case StatusState.Expected:
                    case StatusState.Error:
                    case StatusState.Failure:
                        checkStatus = PullRequestCheckStatus.Failure;
                        break;
                    case StatusState.Pending:
                        checkStatus = PullRequestCheckStatus.Pending;
                        break;
                    case StatusState.Success:
                        checkStatus = PullRequestCheckStatus.Success;
                        break;
                    default:
                        throw new InvalidOperationException("Unkown PullRequestCheckStatusEnum");
                }

                var pullRequestCheckViewModel = (PullRequestCheckViewModel) viewViewModelFactory.CreateViewModel<IPullRequestCheckViewModel>();
                pullRequestCheckViewModel.CheckType = PullRequestCheckType.StatusApi;
                pullRequestCheckViewModel.Title = statusModel.Context;
                pullRequestCheckViewModel.Description = statusModel.Description;
                pullRequestCheckViewModel.Status = checkStatus;
                pullRequestCheckViewModel.DetailsUrl = !string.IsNullOrEmpty(statusModel.TargetUrl) ? new Uri(statusModel.TargetUrl) : null;
                pullRequestCheckViewModel.AvatarUrl = statusModel.AvatarUrl ?? DefaultAvatar;
                pullRequestCheckViewModel.Avatar = statusModel.AvatarUrl != null
                    ? new BitmapImage(new Uri(statusModel.AvatarUrl))
                    : AvatarProvider.CreateBitmapImage(DefaultAvatar);

                return pullRequestCheckViewModel;
            }) ?? Array.Empty<PullRequestCheckViewModel>();

            var checks = pullRequest.CheckSuites?.SelectMany(checkSuiteModel => checkSuiteModel.CheckRuns)
                .Select(checkRunModel =>
                {
                    PullRequestCheckStatus checkStatus;
                    switch (checkRunModel.Status)
                    {
                        case CheckStatusState.Requested:
                        case CheckStatusState.Queued:
                        case CheckStatusState.InProgress:
                            checkStatus = PullRequestCheckStatus.Pending;
                            break;

                        case CheckStatusState.Completed:
                            switch (checkRunModel.Conclusion)
                            {
                                case CheckConclusionState.Success:
                                    checkStatus = PullRequestCheckStatus.Success;
                                    break;

                                case CheckConclusionState.ActionRequired:
                                case CheckConclusionState.TimedOut:
                                case CheckConclusionState.Cancelled:
                                case CheckConclusionState.Failure:
                                case CheckConclusionState.Neutral:
                                    checkStatus = PullRequestCheckStatus.Failure;
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var pullRequestCheckViewModel = (PullRequestCheckViewModel)viewViewModelFactory.CreateViewModel<IPullRequestCheckViewModel>();
                    pullRequestCheckViewModel.CheckType = PullRequestCheckType.ChecksApi;
                    pullRequestCheckViewModel.CheckRunId = checkRunModel.DatabaseId;
                    pullRequestCheckViewModel.HasAnnotations = checkRunModel.Annotations?.Any() ?? false;
                    pullRequestCheckViewModel.Title = checkRunModel.Name;
                    pullRequestCheckViewModel.Description = checkRunModel.Summary;
                    pullRequestCheckViewModel.Status = checkStatus;
                    pullRequestCheckViewModel.DetailsUrl = new Uri(checkRunModel.DetailsUrl);

                    return pullRequestCheckViewModel;
                }) ?? Array.Empty<PullRequestCheckViewModel>();

            return statuses.Concat(checks).OrderBy(model => model.Title);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PullRequestCheckViewModel"/>.
        /// </summary>
        /// <param name="usageTracker">The usage tracker.</param>
        [ImportingConstructor]
        public PullRequestCheckViewModel(IUsageTracker usageTracker)
        {
            this.usageTracker = usageTracker;
            OpenDetailsUrl = ReactiveCommand.Create().OnExecuteCompleted(DoOpenDetailsUrl);
        }

        private void DoOpenDetailsUrl(object obj)
        {
            Expression<Func<UsageModel.MeasuresModel, int>> expression;
            if (CheckType == PullRequestCheckType.StatusApi)
            {
                expression = x => x.NumberOfPRStatusesOpenInGitHub;
            }
            else
            {
                expression = x => x.NumberOfPRChecksOpenInGitHub;
            }

            usageTracker.IncrementCounter(expression).Forget();
        }

        /// <inheritdoc/>
        public string Title { get; private set; }

        /// <inheritdoc/>
        public string Description { get; private set; }

        /// <inheritdoc/>
        public PullRequestCheckType CheckType { get; private set; }

        /// <inheritdoc/>
        public int CheckRunId { get; private set; }

        /// <inheritdoc/>
        public bool HasAnnotations { get; private set; }

        /// <inheritdoc/>
        public PullRequestCheckStatus Status{ get; private set; }

        /// <inheritdoc/>
        public Uri DetailsUrl { get; private set; }

        /// <inheritdoc/>
        public ReactiveCommand<object> OpenDetailsUrl { get; }

        public string AvatarUrl { get; private set; }

        public BitmapImage Avatar { get; private set; }
    }
}
