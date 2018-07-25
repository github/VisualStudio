using System;
using System.Windows.Media.Imaging;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestCheckViewModel: IViewModel
    {
        string Title { get; set; }
        string Description { get; set; }
        PullRequestCheckStatusEnum Status { get; set; }
        Uri DetailsUrl { get; set; }
        string AvatarUrl { get; set; }
        BitmapImage Avatar { get; set; }
        ReactiveCommand<object> OpenDetailsUrl { get; }
    }

    public enum PullRequestCheckStatusEnum
    {
        Pending,
        Success,
        Failure
    }
}