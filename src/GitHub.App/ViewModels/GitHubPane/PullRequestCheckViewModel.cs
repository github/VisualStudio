using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
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
            var statuses = pullRequest.Statuses?.Select(model =>
            {
                PullRequestCheckStatus checkStatus;
                switch (model.State)
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
                pullRequestCheckViewModel.Title = model.Context;
                pullRequestCheckViewModel.Description = model.Description;
                pullRequestCheckViewModel.Status = checkStatus;
                pullRequestCheckViewModel.DetailsUrl = !string.IsNullOrEmpty(model.TargetUrl) ? new Uri(model.TargetUrl) : null;

                return pullRequestCheckViewModel;
            }) ?? Array.Empty<PullRequestCheckViewModel>();

            var checks = pullRequest.CheckSuites?.SelectMany(model => model.CheckRuns)
                .Select(model =>
                {
                    PullRequestCheckStatus checkStatus;
                    switch (model.Status)
                    {
                        case CheckStatusState.Requested:
                        case CheckStatusState.Queued:
                        case CheckStatusState.InProgress:
                            checkStatus = PullRequestCheckStatus.Pending;
                            break;

                        case CheckStatusState.Completed:
                            switch (model.Conclusion)
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
                    pullRequestCheckViewModel.Title = model.Name;

                    var description = new StringBuilder(model.Conclusion.ToString());

                    if (model.StartedAt.HasValue && model.CompletedAt.HasValue)
                    {
                        description.Append(" in ");
                        var timeSpan = model.CompletedAt.Value - model.StartedAt.Value;
                        description.Append(timeSpan.ToString());
                    }

                    if (!string.IsNullOrEmpty(model.Title))
                    {
                        description.Append(" - ");
                        description.Append(model.Title);
                    }

                    pullRequestCheckViewModel.Description = description.ToString();
                    pullRequestCheckViewModel.Status = checkStatus;
                    pullRequestCheckViewModel.DetailsUrl = new Uri(model.DetailsUrl);

                    return pullRequestCheckViewModel;
                }) ?? Array.Empty<PullRequestCheckViewModel>();

            return statuses.Concat(checks).OrderBy(model => model.Title);
        }

        [ImportingConstructor]
        public PullRequestCheckViewModel(IUsageTracker usageTracker)
        {
            this.usageTracker = usageTracker;
            OpenDetailsUrl = ReactiveCommand.Create(DoOpenDetailsUrl);
        }

        private void DoOpenDetailsUrl()
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

        public PullRequestCheckStatus Status{ get; private set; }

        public Uri DetailsUrl { get; private set; }

        public ReactiveCommand<Unit, Unit> OpenDetailsUrl { get; }
    }
}
