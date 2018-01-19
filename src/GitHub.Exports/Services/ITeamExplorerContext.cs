using System;
using System.ComponentModel;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Responsible for watching the active repository in Team Explorer.
    /// A PropertyChanged event is fired when moving to a new repository.
    /// A StatusChanged event is fired when the CurrentBranch or HeadSha changes.
    /// </summary>
    public interface ITeamExplorerContext : INotifyPropertyChanged
    {
        /// <summary>
        /// The active Git repository in Team Explorer.
        /// This will be null if no repository is active.
        /// </summary>
        ILocalRepositoryModel ActiveRepository { get; }

        /// <summary>
        /// Fired when the CurrentBranch or HeadSha changes.
        /// </summary>
        event EventHandler StatusChanged;
    }
}
