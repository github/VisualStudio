using System;
using System.Runtime.InteropServices;
using GitHub.ViewModels.Documents;

namespace GitHub.VisualStudio.UI
{
    [Guid(IssueDetailPaneGuid)]
    public class IssueDetailPane : AsyncPaneBase<IIssueDetailViewModel>
    {
        public const string IssueDetailPaneGuid = "9506846C-4CEC-4DDA-87E7-A99CDCD4E35B";
    }
}
