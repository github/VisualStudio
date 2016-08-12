using GitHub.Primitives;
using GitHub.UI;
using System.ComponentModel;
using System.Threading.Tasks;
using GitHub.Api;

namespace GitHub.Models
{
    public enum RepositoryOrigin
    {
        Unknown,
        DotCom,
        Enterprise,
        Other,
        NonGitRepository,
    }

    public interface ISimpleRepositoryModel : INotifyPropertyChanged
    {
        string Name { get; }
        UriString CloneUrl { get; }
        string LocalPath { get; }
        Octicon Icon { get; }

        void SetIcon(bool isPrivate, bool isFork);

        /// <summary>
        /// Updates the url information based on the local path
        /// </summary>
        void Refresh();

        UriString GenerateUrl(string path = null, int startLine = -1, int endLine = -1);

        /// <summary>
        /// Gets the origin of the repository: whether from github.com, enterprise or other.
        /// </summary>
        /// <param name="apiClient">An API client to use to speak to the remote.</param>
        /// <returns>A task tracking the operation.</returns>
        Task<RepositoryOrigin> GetOrigin(ISimpleApiClient apiClient);
    }
}
