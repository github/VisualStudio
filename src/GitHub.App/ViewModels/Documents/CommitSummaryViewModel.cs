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
            Author = new ActorViewModel(model.Author);
            Header = model.MessageHeadline;
        }

        public string AbbreviatedOid { get; }
        public IActorViewModel Author { get; }
        public string Header { get; }
    }
}
