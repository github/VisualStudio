using System.Collections.Generic;

namespace GitHub.ViewModels.Documents
{
    public class CommitSummariesViewModel : ViewModelBase
    {
        public CommitSummariesViewModel(params CommitSummaryViewModel[] commits)
        {
            Commits = commits;
        }

        public IReadOnlyList<CommitSummaryViewModel> Commits { get; }
    }
}
