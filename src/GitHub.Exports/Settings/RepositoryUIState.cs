using System;
using System.Collections.Generic;

namespace GitHub.Settings
{
    public class RepositoryUIState
    {
        public string RepositoryUrl { get; set; }

        public PullRequestListUIState PullRequests { get; set; }
            = new PullRequestListUIState();
    }
}
