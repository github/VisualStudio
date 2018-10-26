using System;
using System.Runtime.InteropServices;
using GitHub.ViewModels.Documents;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.UI
{
    /// <summary>
    /// A <see cref="ToolWindowPane"/> which displays an issue or pull request in a document window.
    /// </summary>
    [Guid(IssueishDocumentPaneGuid)]
    public class IssueishDocumentPane : AsyncPaneBase<IIssueishPaneViewModel>
    {
        public const string IssueishDocumentPaneGuid = "9506846C-4CEC-4DDA-87E7-A99CDCD4E35B";
    }
}
