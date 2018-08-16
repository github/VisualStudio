using System;
using System.Windows.Media.Imaging;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels.GitHubPane
{
    /// <summary>
    /// Represents a view model for displaying details of a pull request Status or Check.
    /// </summary>
    public interface IPullRequestCheckViewModel: IViewModel
    {
        /// <summary>
        /// The title of the Status/Check
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The description of the Status/Check
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The status of the Status/Check
        /// </summary>
        PullRequestCheckStatus Status { get; }

        /// <summary>
        /// The url where more information about the Status/Check can be found
        /// </summary>
        Uri DetailsUrl { get; }

        /// <summary>
        /// The AvatarUrl of the Status/Check application
        /// </summary>
        string AvatarUrl { get; }

        /// <summary>
        /// The BitmapImage of the AvatarUrl
        /// </summary>
        BitmapImage Avatar { get; }

        /// <summary>
        /// A command that opens the DetailsUrl in a browser
        /// </summary>
        ReactiveCommand<object> OpenDetailsUrl { get; }

        PullRequestCheckType CheckType { get; }

        int CheckRunId { get; }

        bool HasAnnotations { get; }
    }

    public enum PullRequestCheckStatus
    {
        Pending,
        Success,
        Failure
    }
}