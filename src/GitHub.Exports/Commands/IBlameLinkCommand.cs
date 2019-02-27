using System;

namespace GitHub.Commands
{
    /// <summary>
    /// Opens the blame view for the currently selected text on GitHub.com or an Enterprise
    /// instance.
    /// </summary>
    public interface IBlameLinkCommand : IVsCommand
    {
    }
}