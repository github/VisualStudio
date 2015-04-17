using System.Collections.Generic;
using LibGit2Sharp;

public class FakeCommitLog :  List<Commit>, ICommitLog
{
    public CommitSortStrategies SortedBy
    {
        get
        {
            return CommitSortStrategies.Topological;
        }
    }
}
