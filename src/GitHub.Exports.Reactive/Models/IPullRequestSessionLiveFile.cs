using System;
using System.Collections.Generic;

namespace GitHub.Models
{
    public interface IPullRequestSessionLiveFile : IPullRequestSessionFile
    {
        IObservable<IList<int>> LinesChanged { get; }
    }
}
