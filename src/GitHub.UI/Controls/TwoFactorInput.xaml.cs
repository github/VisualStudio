using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitHub.UI
{
    /// <summary>
    /// Interaction logic for TwoFactorInput.xaml
    /// </summary>
    public partial class TwoFactorInput : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TwoFactorInput), new PropertyMetadata(""));

        public TwoFactorInput()
        {
            InitializeComponent();

            SetupTextBox(one);
            SetupTextBox(two);
            SetupTextBox(three);
            SetupTextBox(four);
            SetupTextBox(five);
            SetupTextBox(six);
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            private set { SetValue(TextProperty, value); }
        }

        private void SetupTextBox(TextBox textBox)
        {
            textBox.GotFocus += (sender, args) => textBox.SelectAll();

            textBox.PreviewKeyDown += (sender, args) =>
            {
                if (args.Key != Key.D0
                    && args.Key != Key.D1
                    && args.Key != Key.D2
                    && args.Key != Key.D3
                    && args.Key != Key.D4
                    && args.Key != Key.D5
                    && args.Key != Key.D6
                    && args.Key != Key.D7
                    && args.Key != Key.D8
                    && args.Key != Key.D9
                    && args.Key != Key.Tab
                    && args.Key != Key.Escape)
                {
                    args.Handled = true;
                }
            };

            textBox.TextChanged += (sender, args) =>
            {
                var tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                var keyboardFocus = Keyboard.FocusedElement as UIElement;

                Text = GetTwoFactorCode();

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
            };
        }

        private string GetTextBoxValue(TextBox textBox)
        {
            return String.IsNullOrEmpty(textBox.Text) ? " " : textBox.Text;
        }

        private string GetTwoFactorCode()
        {
            return GetTextBoxValue(one)
                   + GetTextBoxValue(two)
                   + GetTextBoxValue(three)
                   + GetTextBoxValue(four)
                   + GetTextBoxValue(five)
                   + GetTextBoxValue(six);
        }
    }
}
