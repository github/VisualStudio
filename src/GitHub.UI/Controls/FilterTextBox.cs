using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GitHub.UI
{
    public class FilterTextBox : TextBox
    {
        public static readonly DependencyProperty PromptTextProperty =
            DependencyProperty.Register("PromptText", typeof(string), typeof(FilterTextBox), new UIPropertyMetadata("Filter"));

        [Localizability(LocalizationCategory.Text)]
        [DefaultValue("Filter")]
        public string PromptText
        {
            get { return (string)GetValue(PromptTextProperty); }
            set { SetValue(PromptTextProperty, value); }
        }

        public FilterTextBox()
        {
            // http://stackoverflow.com/a/661224/2114
            AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);
            AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText), true);
            AddHandler(Button.ClickEvent, new RoutedEventHandler(ClearButtonClick), true);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && !String.IsNullOrEmpty(Text))
            {
                Clear();
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            Clear();
            e.Handled = true;
        }

        // http://stackoverflow.com/a/661224/2114
        static void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            var textBox = FindTextBoxInAncestors(e.OriginalSource as UIElement);
            
            if (textBox != null && !textBox.IsKeyboardFocusWithin)
            {
                // If the text box is not yet focussed, give it the focus and
                // stop further processing of this click event.
                textBox.Focus();
                e.Handled = true;
            }
        }

        static TextBox FindTextBoxInAncestors(DependencyObject current)
        {
            while (current != null)
            {
                var tb = current as TextBox;
                if (tb != null)
                    return tb;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        static void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }
    }
}
