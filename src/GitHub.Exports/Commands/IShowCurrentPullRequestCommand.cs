using System;

namespace GitHub.Commands
{
    /// <summary>
    /// Opens the GitHub pane and shows the currently checked out pull request.
    /// </summary>
    /// <remarks>
    /// Does nothing if there is no checked out pull request.
    /// </remarks>
    public interface IShowCurrentPullRequestCommand : IVsCommand
    {
    }
}