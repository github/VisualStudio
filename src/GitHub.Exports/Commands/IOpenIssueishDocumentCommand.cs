using System;

namespace GitHub.Commands
{
    /// <summary>
    /// Opens an issue or pull request in a new document window.
    /// </summary>
    public interface IOpenIssueishDocumentCommand : IVsCommand<OpenIssueishParams>
    {
    }
}