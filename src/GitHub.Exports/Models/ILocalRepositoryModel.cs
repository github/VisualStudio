using System.ComponentModel;

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
    }
}
