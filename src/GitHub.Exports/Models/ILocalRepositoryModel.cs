using GitHub.Exports;
using GitHub.Primitives;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GitHub.Models
{
    /// <summary>
    /// Represents a locally cloned repository.
    /// </summary>
    public interface ILocalRepositoryModel : IRepositoryModel, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the path to the repository on the filesystem.
        /// </summary>
        string LocalPath { get; }

        /// <summary>
        /// Gets the current branch.
        /// </summary>
        IBranch CurrentBranch { get; }

        /// <summary>
        /// Updates the url information based on the local path
        /// </summary>
        void Refresh();
    }
}
