using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub.VisualStudio.TeamExplorerHome
{
    public interface IGitHubHomeSection
    {
        string RepoName
        {
            get;
            set;
        }

        string RepoUrl
        {
            get;
            set;
        }
    }
}
