using System.Collections.Generic;
using System.Linq;

namespace GitHub.ViewModels.Documents
{
    public class CommitSummariesViewModel : ViewModelBase
    {
        public CommitSummariesViewModel(params CommitSummaryViewModel[] commits)
        {
            Commits = commits;
        }

        public CommitSummariesViewModel(IEnumerable<CommitSummaryViewModel> commits)
        {
            Commits = commits.ToList();
        }

        public IReadOnlyList<CommitSummaryViewModel> Commits { get; }
    }
}
