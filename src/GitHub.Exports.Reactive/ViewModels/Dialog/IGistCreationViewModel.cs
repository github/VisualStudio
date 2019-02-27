using System.Reactive;
using GitHub.Models;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels.Dialog
{
    public interface IGistCreationViewModel : IDialogContentViewModel, IConnectionInitializedViewModel
    {
        /// <summary>
        /// Gets the command to create a new gist.
        /// </summary>
        ReactiveCommand<Unit, Gist> CreateGist { get; }

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

        /// <summary>
        /// The account or organization that will be the owner of the created gist.
        /// </summary>
        IAccount Account { get; }
    }
}
