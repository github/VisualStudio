using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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
            return pullRequest.Statuses?.Select(model =>
            {
                PullRequestCheckStatus checkStatus;
                switch (model.State)
                {
                    case StatusStateEnum.Expected:
                    case StatusStateEnum.Error:
                    case StatusStateEnum.Failure:
                        checkStatus = PullRequestCheckStatus.Failure;
                        break;
                    case StatusStateEnum.Pending:
                        checkStatus = PullRequestCheckStatus.Pending;
                        break;
                    case StatusStateEnum.Success:
                        checkStatus = PullRequestCheckStatus.Success;
                        break;
                    default:
                        throw new InvalidOperationException("Unkown PullRequestCheckStatusEnum");
                }

                var pullRequestCheckViewModel = (PullRequestCheckViewModel) viewViewModelFactory.CreateViewModel<IPullRequestCheckViewModel>();
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
        }

        [ImportingConstructor]
        public PullRequestCheckViewModel(IUsageTracker usageTracker)
        {
            this.usageTracker = usageTracker;
            OpenDetailsUrl = ReactiveCommand.Create().OnExecuteCompleted(DoOpenDetailsUrl);
        }

        private void DoOpenDetailsUrl(object obj)
        {
            usageTracker.IncrementCounter(x => x.NumberOfPRCheckStatusesOpenInGitHub).Forget();
        }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public PullRequestCheckStatus Status{ get; private set; }

        public Uri DetailsUrl { get; private set; }

        public string AvatarUrl { get; private set; }

        public BitmapImage Avatar { get; private set; }

        public ReactiveCommand<object> OpenDetailsUrl { get; }
    }
}
