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
        ILocalBranch CurrentBranch { get; }

        /// <summary>
        /// Updates the url information based on the local path
        /// </summary>
        void Refresh();

        /// <summary>
        /// Generates a http(s) url to the repository in the remote server, optionally
        /// pointing to a specific file and specific line range in it.
        /// </summary>
        /// <param name="path">The file to generate an url to. Optional.</param>
        /// <param name="startLine">A specific line, or (if specifying the <paramref name="endLine"/> as well) the start of a range</param>
        /// <param name="endLine">The end of a line range on the specified file.</param>
        /// <returns>An UriString with the generated url, or null if the repository has no remote server configured or if it can't be found locally</returns>
        Task<UriString> GenerateUrl(LinkType linkType, string path = null, int startLine = -1, int endLine = -1);
    }
}
