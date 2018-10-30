using System;
using System.Reactive;
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
        /// The title of the Status/Check.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The description of the Status/Check.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The status of the Status/Check.
        /// </summary>
        PullRequestCheckStatus Status { get; }

        /// <summary>
        /// The url where more information about the Status/Check can be found.
        /// </summary>
        Uri DetailsUrl { get; }

        /// <summary>
        /// The amount of time this Status/Check took to run.
        /// </summary>
        string DurationStatus { get; }

        /// <summary>
        /// A command that opens the DetailsUrl in a browser
        /// </summary>
        ReactiveCommand<Unit, Unit> OpenDetailsUrl { get; }
    }

    public enum PullRequestCheckStatus
    {
        Pending,
        Success,
        Failure
    }
}