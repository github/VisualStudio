using System;
using GitHub.Models;

namespace GitHub.Services
{
    public interface ITeamExplorerContext
    {
        ILocalRepositoryModel GetActiveRepository();
        event EventHandler StatusChanged;
    }
}
