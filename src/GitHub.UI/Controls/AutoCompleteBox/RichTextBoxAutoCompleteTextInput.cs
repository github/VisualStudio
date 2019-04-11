using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace GitHub.UI
{
    [ContentProperty("TextBox")]
    public class RichTextBoxAutoCompleteTextInput : IAutoCompleteTextInput
    {
        private static readonly int newLineLength = Environment.NewLine.Length;
        const int promptRichTextBoxCaretIndexAdjustments = 2;
        RichTextBox textBox;

        public event PropertyChangedEventHandler PropertyChanged;

        TextPointer ContentStart
        {
            get { return textBox.Document.ContentStart; }
        }

        TextPointer ContentEnd
        {
            get
            {
                // RichTextBox always appends a new line at the end. So we need to back that shit up.
                return textBox.Document.ContentEnd.GetPositionAtOffset(-1 * newLineLength)
                       ?? textBox.Document.ContentEnd;
            }
        }

        public void Select(int position, int length)
        {
            var textRange = new TextRange(ContentStart, ContentEnd);

            if (textRange.Text.Length >= (position + length))
            {
                var start = textRange.Start.GetPositionAtOffset(GetOffsetIndex(position), LogicalDirection.Forward);
                var end = textRange.Start.GetPositionAtOffset(GetOffsetIndex(position + length), LogicalDirection.Backward);
                if (start != null && end != null)
                    textBox.Selection.Select(start, end);
            }
        }

        public void SelectAll()
        {
            textBox.Selection.Select(ContentStart, ContentEnd);
        }

        public int CaretIndex
        {
            get
            {
                var start = ContentStart;
                var caret = textBox.CaretPosition;
                var range = new TextRange(start, caret);
                return range.Text.Length;
            }
            set
            {
                Select(value, 0);
                Debug.Assert(value == CaretIndex,
                    String.Format(CultureInfo.InvariantCulture,
                    "I just set the caret index to '{0}' but it's '{1}'", value, CaretIndex));
            }
        }

        public int SelectionStart
        {
            get
            {
                return new TextRange(ContentStart, textBox.Selection.Start).Text.Length;
            }
        }

        public int SelectionLength
        {
            get { return CaretIndex - SelectionStart; }
        }

#if DEBUG
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
#endif
        public string Text
        {
            get
            {
                return new TextRange(ContentStart, ContentEnd).Text;
            }
            set
            {
                textBox.Document.Blocks.Clear();

                if (!string.IsNullOrEmpty(value))
                {
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
                    {
                        var contents = new TextRange(ContentStart, ContentEnd);
                        contents.Load(stream, DataFormats.Text);
                    }
                }
            }
        }

        public IObservable<EventPattern<KeyEventArgs>> PreviewKeyDown
        {
            get;
            private set;
        }

        public IObservable<EventPattern<RoutedEventArgs>> SelectionChanged { get; private set; }

        public IObservable<EventPattern<TextChangedEventArgs>> TextChanged { get; private set; }

        public UIElement Control { get { return textBox; } }

        public Point GetPositionFromCharIndex(int charIndex)
        {
            var offset = new TextRange(ContentStart, textBox.CaretPosition)
                .Start
                .GetPositionAtOffset(charIndex, LogicalDirection.Forward);

            return offset != null
                ? offset.GetCharacterRect(LogicalDirection.Forward).BottomLeft
                : new Point(0, 0);
        }

        public Thickness Margin
        {
            get { return textBox.Margin; }
            set { textBox.Margin = value; }
        }

        public void Focus()
        {
            Keyboard.Focus(textBox);
        }

        public RichTextBox TextBox
        {
            get
            {
                return textBox;
            }
            set
            {
                if (value != textBox)
                {
                    textBox = value;

                    PreviewKeyDown = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                        h => textBox.PreviewKeyDown += h,
                        h => textBox.PreviewKeyDown -= h);

                    SelectionChanged = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                        h => textBox.SelectionChanged += h,
                        h => textBox.SelectionChanged -= h);

                    TextChanged = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                        h => textBox.TextChanged += h,
                        h => textBox.TextChanged -= h);

                    NotifyPropertyChanged("Control");
                }

            }
        }

        // This is a fudge factor needed because of PromptRichTextBox. When commit messages are 51 characters or more,
        // The PromptRichTextBox applies a styling that fucks up the CaretPosition by 2. :(
        // This method helps us account for that.
        int GetOffsetIndex(int selectionEnd)
        {
            if (textBox is PromptRichTextBox && selectionEnd >= PromptRichTextBox.BadCommitMessageLength)
            {
                return selectionEnd + promptRichTextBoxCaretIndexAdjustments;
            }
            return selectionEnd;
        }

        private void NotifyPropertyChanged(String info)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
