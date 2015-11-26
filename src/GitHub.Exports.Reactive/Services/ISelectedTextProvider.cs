using System;

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
        /// <returns></returns>
        IObservable<string> GetSelectedText();
    }
}
