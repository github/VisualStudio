using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using NullGuard;

namespace GitHub.UI
{
    public class PromptTextBox : TextBox, IShortcutContainer
    {
        public static readonly DependencyProperty PromptTextProperty =
            DependencyProperty.Register("PromptText", typeof(string), typeof(PromptTextBox), new UIPropertyMetadata(""));

        [Localizability(LocalizationCategory.Text)]
        [DefaultValue("")]
        public string PromptText
        {
            [return: AllowNull]
            get { return (string)GetValue(PromptTextProperty); }
            set { SetValue(PromptTextProperty, value); }
        }

        [DefaultValue(true)]
        public bool SupportsKeyboardShortcuts { get; set; }
    }
}
