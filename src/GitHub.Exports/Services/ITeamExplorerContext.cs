using System;
using System.ComponentModel;
using GitHub.Models;

namespace GitHub.Services
{
    /// <summary>
    /// Responsible for watching the active repository in Team Explorer.
    /// </summary>
    /// <remarks>
    /// A <see cref="PropertyChanged"/> event is fired when moving to a new repository.
    /// A <see cref="StatusChanged"/> event is fired when the current branch, head SHA or tracked SHA changes.
    /// </remarks>
    public interface ITeamExplorerContext : INotifyPropertyChanged
    {
        /// <summary>
        /// The active Git repository in Team Explorer.
        /// This will be null if no repository is active.
        /// </summary>
        /// <remarks>
        /// This property might be changed by a non-UI thread.
        /// </remarks>
        LocalRepositoryModel ActiveRepository { get; }

        /// <summary>
        /// Fired when the CurrentBranch or HeadSha changes.
        /// </summary>
        /// <remarks>
        /// This event might fire on a non-UI thread.
        /// </remarks>
        event EventHandler StatusChanged;
    }
}
