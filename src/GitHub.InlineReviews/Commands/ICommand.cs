using System;
using Microsoft.VisualStudio.Shell;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Represents a command registered on package initialization.
    /// </summary>
    interface ICommand
    {
        /// <summary>
        /// Registers the command with a package.
        /// </summary>
        /// <param name="package">The package registering the command.</param>
        void Register(IServiceProvider package);
    }
}
