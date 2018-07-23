using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media.Imaging;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.ViewModels.GitHubPane
{
    [Export(typeof(IPullRequestCheckViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PullRequestCheckViewModel: IPullRequestCheckViewModel
    {
        const string DefaultAvatar = "pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png";

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
                    DetailsUrl = model.TargetUrl,
                    AvatarUrl = model.AvatarUrl ?? DefaultAvatar,
                    Avatar = model.AvatarUrl != null
                        ? new BitmapImage(new Uri(model.AvatarUrl))
                        : AvatarProvider.CreateBitmapImage(DefaultAvatar)
                };

            }) ?? new PullRequestCheckViewModel[0];
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public PullRequestCheckStatusEnum Status { get; set; }

        public string DetailsUrl { get; set; }

        public string AvatarUrl { get; set; }

        public BitmapImage Avatar { get; set; }
    }
}
