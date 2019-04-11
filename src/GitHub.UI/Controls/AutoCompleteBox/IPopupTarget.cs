
using System.Windows;

namespace GitHub.UI.Controls
{
    /// <summary>
    /// Controls that implement this interface can specify where an associated popup should be located.
    /// </summary>
    /// <remarks>
    /// The PopupHelper is a generic class for managing Popups that align to the bottom of their associated control.
    /// However, our AutoCompleteBox needs the Popup to align to where the completion is happening. Intellisense™
    /// controls behave in a similar fashion. We might find popups useful elsewhere.
    /// </remarks>
    public interface IPopupTarget
    {
        Point PopupPosition { get; }
    }
}
