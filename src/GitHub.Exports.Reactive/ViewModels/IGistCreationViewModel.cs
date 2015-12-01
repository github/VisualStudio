using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IGistCreationViewModel : IViewModel
    {
        /// <summary>
        /// Gets the command to create a new public gist.
        /// </summary>
        ReactiveCommand<object> CreatePublicCommand { get; }
        /// <summary>
        /// Gets the command to create a new private gist.
        /// </summary>
        ReactiveCommand<object> CreatePrivateCommand { get; }
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
