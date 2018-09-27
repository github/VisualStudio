using System;

namespace GitHub.Commands
{
    /// <summary>
    /// Opens the issue details pane with the specified issue.
    /// </summary>
    public interface IShowIssueDetailsCommand : IVsCommand<ShowIssueDetailsParams>
    {
    }
}