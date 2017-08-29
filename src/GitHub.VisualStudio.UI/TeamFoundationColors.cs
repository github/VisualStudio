using System;
using System.ComponentModel;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.UI
{
    /// <summary>
    /// Retrieves themed Team Foundation colors.
    /// </summary>
    public class TeamFoundationColors : INotifyPropertyChanged
    {
        static readonly Guid VsTeamFoundationColorsCategory = new Guid("4aff231b-f28a-44f0-a66b-1beeb17cb920");
        static Lazy<TeamFoundationColors> instance = new Lazy<TeamFoundationColors>(() => new TeamFoundationColors());

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamFoundationColors"/> class.
        /// </summary>
        private TeamFoundationColors()
        {
            VSColorTheme.ThemeChanged += _ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="TeamFoundationColors"/> class.
        /// </summary>
        public static TeamFoundationColors Instance => instance.Value;

        /// <summary>
        /// Gets the Team Foundation "RequiredTextBoxBorder" color.
        /// </summary>
        public Color RequiredTextBoxBorderColor => GetColor("RequiredTextBoxBorder");

        /// <summary>
        /// Gets the Team Foundation "TextBoxBorder" color.
        /// </summary>
        public Color TextBoxBorderColor => GetColor("TextBoxBorder");

        /// <summary>
        /// Gets the Team Foundation "TextBox" color.
        /// </summary>
        public Color TextBoxColor => GetColor("TextBox");

        /// <summary>
        /// Gets the Team Foundation "TextBoxHintText" color.
        /// </summary>
        public Color TextBoxHintTextColor => GetColor("TextBoxHintText");

        /// <summary>
        /// Occurs when a property on the object changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private static Color GetColor(string name)
        {
            var c = VSColorTheme.GetThemedColor(new ThemeResourceKey(VsTeamFoundationColorsCategory, name, ThemeResourceKeyType.BackgroundColor));
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }
}
