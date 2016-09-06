using GitHub.Primitives;
using GitHub.UI;
using System.ComponentModel;

namespace GitHub.Models
{
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

        UriString GenerateUrl(string path = null, int startLine = -1, int endLine = -1);
    }
}
