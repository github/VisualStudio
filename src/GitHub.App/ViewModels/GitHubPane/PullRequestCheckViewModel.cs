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
                pullRequestCheckViewModel.DetailsUrl = new Uri(model.TargetUrl);
                pullRequestCheckViewModel.AvatarUrl = model.AvatarUrl ?? DefaultAvatar;
                pullRequestCheckViewModel.Avatar = model.AvatarUrl != null
                    ? new BitmapImage(new Uri(model.AvatarUrl))
                    : AvatarProvider.CreateBitmapImage(DefaultAvatar);

                return pullRequestCheckViewModel;
            }) ?? new PullRequestCheckViewModel[0];

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
                    pullRequestCheckViewModel.Description = model.Summary;
                    pullRequestCheckViewModel.Status = checkStatus;
                    pullRequestCheckViewModel.DetailsUrl = new Uri(model.DetailsUrl);

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

        public PullRequestCheckStatus Status{ get; private set; }

        public Uri DetailsUrl { get; private set; }

        public string AvatarUrl { get; private set; }

        public BitmapImage Avatar { get; private set; }

        public ReactiveCommand<object> OpenDetailsUrl { get; }
    }
}
