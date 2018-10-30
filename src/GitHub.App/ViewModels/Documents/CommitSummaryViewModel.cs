using System;
using GitHub.Models;
using GitHub.ViewModels;

namespace GitHub.ViewModels.Documents
{
    public class CommitSummaryViewModel : ViewModelBase
    {
        public CommitSummaryViewModel(CommitModel model)
        {
            AbbreviatedOid = model.AbbreviatedOid;
            Header = model.MessageHeadline;
        }

        public string AbbreviatedOid { get; }
        public string Header { get; }
    }
}
