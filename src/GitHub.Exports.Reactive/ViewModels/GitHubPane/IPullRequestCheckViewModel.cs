using System.Windows.Media.Imaging;
using GitHub.Models;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestCheckViewModel
    {
        string Title { get; set; }
        string Description { get; set; }
        PullRequestCheckStatusEnum Status { get; set; }
        string DetailsUrl { get; set; }
        string AvatarUrl { get; set; }
        BitmapImage Avatar { get; set; }
    }

    public enum PullRequestCheckStatusEnum
    {
        Pending,
        Success,
        Failure
    }
}