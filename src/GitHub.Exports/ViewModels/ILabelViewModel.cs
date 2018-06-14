using System.Windows.Media;

namespace GitHub.ViewModels
{
    /// <summary>
    /// View model for an issue/pull request label.
    /// </summary>
    public interface ILabelViewModel
    {
        /// <summary>
        /// Gets the background brush for the label.
        /// </summary>
        Brush BackgroundBrush { get; }

        /// <summary>
        /// Gets the foreground brush for the label.
        /// </summary>
        Brush ForegroundBrush { get; }

        /// <summary>
        /// Gets the name of the label.
        /// </summary>
        string Name { get; }
    }
}