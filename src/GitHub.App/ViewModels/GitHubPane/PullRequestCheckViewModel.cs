using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Windows.Media.Imaging;
using GitHub.Models;
using GitHub.Services;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IPullRequestCheckViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestCheckViewModel: ViewModelBase, IPullRequestCheckViewModel
    {
        const string DefaultAvatar = "pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png";

        private string title;
        private string description;
        private PullRequestCheckStatusEnum status;
        private Uri detailsUrl;
        private string avatarUrl;
        private BitmapImage avatar;

        public static IEnumerable<PullRequestCheckViewModel> Build(PullRequestDetailModel pullRequest)
        {
            return pullRequest.Statuses?.Select(model =>
            {
                var statusStateEnum = model.State;

                PullRequestCheckStatusEnum checkStatus;
                switch (statusStateEnum)
                {
                    case StatusStateEnum.Expected:
                    case StatusStateEnum.Error:
                    case StatusStateEnum.Failure:
                        checkStatus = PullRequestCheckStatusEnum.Failure;
                        break;
                    case StatusStateEnum.Pending:
                        checkStatus = PullRequestCheckStatusEnum.Pending;
                        break;
                    case StatusStateEnum.Success:
                        checkStatus = PullRequestCheckStatusEnum.Success;
                        break;
                    default:
                        throw new InvalidOperationException("Unkown PullRequestCheckStatusEnum");
                }
                
                return new PullRequestCheckViewModel
                {
                    Title = model.Context,
                    Description = model.Description,
                    Status = checkStatus,
                    DetailsUrl = new Uri(model.TargetUrl),
                    AvatarUrl = model.AvatarUrl ?? DefaultAvatar,
                    Avatar = model.AvatarUrl != null
                        ? new BitmapImage(new Uri(model.AvatarUrl))
                        : AvatarProvider.CreateBitmapImage(DefaultAvatar)
                };

            }) ?? new PullRequestCheckViewModel[0];
        }

        public PullRequestCheckViewModel()
        {
            OpenDetailsUrl = ReactiveCommand.Create();
        }

        public string Title
        {
            get { return title; }
            set { this.RaiseAndSetIfChanged(ref title, value); }
        }

        public string Description
        {
            get { return description; }
            set { this.RaiseAndSetIfChanged(ref description, value); }
        }

        public PullRequestCheckStatusEnum Status
        {
            get { return status; }
            set { this.RaiseAndSetIfChanged(ref status, value); }
        }

        public Uri DetailsUrl
        {
            get { return detailsUrl; }
            set { this.RaiseAndSetIfChanged(ref detailsUrl, value); }
        }

        public string AvatarUrl
        {
            get { return avatarUrl; }
            set { this.RaiseAndSetIfChanged(ref avatarUrl, value); }
        }

        public BitmapImage Avatar
        {
            get { return avatar; }
            set { this.RaiseAndSetIfChanged(ref avatar, value); }
        }

        public ReactiveCommand<object> OpenDetailsUrl { get; }
    }
}
