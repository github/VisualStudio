using System;
using System.Windows.Media.Imaging;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestCheckViewModel: IViewModel
    {
        string Title { get; }
        string Description { get; }
        PullRequestCheckStatus Status { get; }
        Uri DetailsUrl { get; }
        string AvatarUrl { get; }
        BitmapImage Avatar { get; }
        ReactiveCommand<object> OpenDetailsUrl { get; }
    }

    public enum PullRequestCheckStatus
    {
        Pending,
        Success,
        Failure
    }
}