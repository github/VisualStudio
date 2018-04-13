using System;
using System.Threading.Tasks;

namespace GitHub.Services
{
    /// <summary>
    /// Provides a way to get any currently selected text in the text editor area.
    /// </summary>
    public interface ISelectedTextProvider
    {
        /// <summary>
        /// Gets the currently selected text.
        /// </summary>
        /// <returns>The selected text in the active editor, or an empty string if no text is selected.</returns>
        string GetSelectedText();
    }
}
