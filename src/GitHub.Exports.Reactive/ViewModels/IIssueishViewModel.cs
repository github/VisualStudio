using System;
using System.Reactive;
using GitHub.Models;
using ReactiveUI;

namespace GitHub.ViewModels
{
    /// <summary>
    /// Base interface for issue and pull request view models.
    /// </summary>
    public interface IIssueishViewModel : IViewModel
    {
        /// <summary>
        /// Gets the issue or pull request author.
        /// </summary>
        IActorViewModel Author { get; }

        /// <summary>
        /// Gets the issue or pull request body.
        /// </summary>
        string Body { get; }

        /// <summary>
        /// Gets the issue or pull request number.
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Gets the repository that the issue or pull request comes from.
        /// </summary>
        IRemoteRepositoryModel Repository { get; }

        /// <summary>
        /// Gets the issue or pull request title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the URL of the issue or pull request.
        /// </summary>
        Uri WebUrl { get; }

        /// <summary>
        /// Gets a command which opens the issue or pull request in a browser.
        /// </summary>
        ReactiveCommand<Unit, Unit> OpenOnGitHub { get; }
    }
}