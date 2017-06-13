using System;
using System.Windows.Input;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Represents a Visual Studio command.
    /// </summary>
    interface IVsCommandBase : IPackageResource, ICommand
    {
        /// <summary>
        /// Gets a value indicating whether the command is enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether the command is visible.
        /// </summary>
        bool IsVisible { get; }
    }
}
