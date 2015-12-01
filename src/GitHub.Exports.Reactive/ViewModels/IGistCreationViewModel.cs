using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IGistCreationViewModel : IViewModel
    {
        /// <summary>
        /// Gets the command to create a new gist.
        /// </summary>
        ReactiveCommand<object> CreateCommand { get; }
        /// <summary>
        /// Gets the command to cancel the creation of a gist.
        /// </summary>
        ReactiveCommand<object> CancelCommand { get; }
        /// <summary>
        /// Gets true if the gist should be public.
        /// </summary>
        bool IsPublic { get; }
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
