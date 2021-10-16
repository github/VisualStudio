using System;

namespace GitHub.Commands
{
    /// <summary>
    /// Copies a link to the clipboard of the currently selected text on GitHub.com or an
    /// Enterprise instance.
    /// </summary>
    public interface ICopyLinkCommand : IVsCommand
    {
    }
}