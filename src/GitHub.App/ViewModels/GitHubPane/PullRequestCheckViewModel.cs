using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
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
        const string DefaultAvatar = "pack://application:,,,/GitHub.App;component/Images/default_user_avatar.png";

        [Import]
        public IVisualStudioBrowser VisualStudioBrowser { get; set; }

        private string title;
        private string description;
        private PullRequestCheckStatusEnum status;
        private Uri detailsUrl;
        private string avatarUrl;
        private BitmapImage avatar;

        public static IEnumerable<IPullRequestCheckViewModel> Build(IViewViewModelFactory viewViewModelFactory,
            PullRequestDetailModel pullRequest)
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

                var pullRequestCheckViewModel = viewViewModelFactory.CreateViewModel<IPullRequestCheckViewModel>();
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

        public PullRequestCheckViewModel()
        {
            OpenDetailsUrl = ReactiveCommand.Create().OnExecuteCompleted(DoOpenDetailsUrl);
        }

        private void DoOpenDetailsUrl(object obj)
        {
            VisualStudioBrowser.OpenUrl(DetailsUrl);
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
