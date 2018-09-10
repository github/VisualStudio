using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using GitHub.Extensions;
using GitHub.Factories;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IPullRequestCheckViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestCheckViewModel: ViewModelBase, IPullRequestCheckViewModel
    {
        private readonly IUsageTracker usageTracker;
        const string DefaultAvatar = "pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png";

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

                return pullRequestCheckViewModel;
            }) ?? new PullRequestCheckViewModel[0];

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
                    pullRequestCheckViewModel.CheckRunId = checkRunModel.CheckRunId;
                    pullRequestCheckViewModel.HasAnnotations = checkRunModel.Annotations?.Any() ?? false;
                    pullRequestCheckViewModel.Title = checkRunModel.Name;
                    pullRequestCheckViewModel.Description = checkRunModel.Summary;
                    pullRequestCheckViewModel.Status = checkStatus;
                    pullRequestCheckViewModel.DetailsUrl = new Uri(checkRunModel.DetailsUrl);

                    return pullRequestCheckViewModel;
                }) ?? new PullRequestCheckViewModel[0];

            return statuses.Concat(checks).OrderBy(model => model.Title);
        }

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
        
        public string Title { get; private set; }

        public string Description { get; private set; }

        public PullRequestCheckType CheckType { get; private set; }

        public int CheckRunId { get; private set; }

        public bool HasAnnotations { get; private set; }

        public PullRequestCheckStatus Status{ get; private set; }

        public Uri DetailsUrl { get; private set; }

        public ReactiveCommand<object> OpenDetailsUrl { get; }
    }
}
