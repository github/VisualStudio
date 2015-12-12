using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IGistCreationViewModel : IViewModel
    {
        /// <summary>
        /// Gets the command to create a new gist.
        /// </summary>
        IReactiveCommand<Gist> CreateGist { get; }

        /// <summary>
        /// True if the gist should be private.
        /// </summary>
        bool IsPrivate { get; set; }

        /// <summary>
        /// Gets or sets the optional description used in the gist description field.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the file name of the gist (should include extension).
        /// </summary>
        string FileName { get; set; }
    }
}
