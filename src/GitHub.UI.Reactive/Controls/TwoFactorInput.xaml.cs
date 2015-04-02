using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.Extensions;
using NullGuard;
using System.Globalization;

namespace GitHub.UI
{
    public class TwoFactorInputToTextBox : ValueConverterMarkupExtension<TwoFactorInputToTextBox>
    {
        public override object Convert([AllowNull] object value, [AllowNull] Type targetType,
            [AllowNull] object parameter, [AllowNull] CultureInfo culture)
        {
            return value is TwoFactorInput ? ((TwoFactorInput)value).TextBox : null;
        }
    }

    /// <summary>
    /// Interaction logic for TwoFactorInput.xaml
    /// </summary>
    public partial class TwoFactorInput : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TwoFactorInput), new PropertyMetadata(""));

        TextBox[] TextBoxes;
        public TextBox TextBox { get { return TextBoxes[0]; } }

        public TwoFactorInput()
        {
            InitializeComponent();

            TextBoxes = new[]
            {
                one,
                two,
                three,
                four,
                five,
                six
            };

            foreach(var textBox in TextBoxes)
            {
                SetupTextBox(textBox);
            }
        }

        public IObservable<bool> TryFocus()
        {
            return one.TryMoveFocus(FocusNavigationDirection.First);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.Text, true);
            if (!isText) return;

            var text = e.SourceDataObject.GetData(DataFormats.Text) as string;
            if (text == null) return;
            e.CancelCommand();
            SetText(text);
        }

        void SetText(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                foreach (var textBox in TextBoxes)
                {
                    textBox.Text = "";
                }
                SetValue(TextProperty, text);
                return;
            }
            var digits = text.Where(Char.IsDigit).ToList();
            for (int i = 0; i < Math.Min(6, digits.Count); i++)
            {
                TextBoxes[i].Text = digits[i].ToString();
            }
            SetValue(TextProperty, String.Join("", digits));
        }

        [AllowNull]
        public string Text
        {
            [return: AllowNull]
            get { return (string)GetValue(TextProperty); }
            set { SetText(value); }
        }

        private void SetupTextBox(TextBox textBox)
        {
            DataObject.AddPastingHandler(textBox, new DataObjectPastingEventHandler(OnPaste));

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
                    && args.Key != Key.Escape
                    && (!(args.Key == Key.V && args.KeyboardDevice.Modifiers == ModifierKeys.Control))
                    && (!(args.Key == Key.Insert && args.KeyboardDevice.Modifiers == ModifierKeys.Shift)))
                {
                    args.Handled = true;
                }
            };

            textBox.SelectionChanged += (sender, args) =>
            {
                // Make sure we can't insert additional text into a textbox.
                // Each textbox should only allow one character.
                if (textBox.SelectionLength == 0 && textBox.Text.Any())
                {
                    textBox.SelectAll();
                }
            };

            textBox.TextChanged += (sender, args) =>
            {
                var tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                var keyboardFocus = Keyboard.FocusedElement as UIElement;

                SetValue(TextProperty, String.Join("", GetTwoFactorCode()));

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
            };
        }

        private static string GetTextBoxValue(TextBox textBox)
        {
            return String.IsNullOrEmpty(textBox.Text) ? " " : textBox.Text;
        }

        private string GetTwoFactorCode()
        {
            return String.Join("", TextBoxes.Select(textBox => textBox.Text));
        }
    }
}
