using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IGistCreationViewModel : IViewModel
    {
        /// <summary>
        /// Gets the command to create a new public gist.
        /// </summary>
        IReactiveCommand<Gist> CreatePublicGist { get; }
        /// <summary>
        /// Gets the command to create a new private gist.
        /// </summary>
        IReactiveCommand<Gist> CreatePrivateGist { get; }
        /// <summary>
        /// Gets the optional description used in the gist description field .
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Gets the main content of the gist.
        /// </summary>
        string Content { get; }
        /// <summary>
        /// Gets the gist filename including extension.
        /// </summary>
        string FileName { get; }
    }
}
