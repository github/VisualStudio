using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ReactiveUI.Wpf;
using ReactiveUI;

namespace GitHub.UI
{
    [ContentProperty("TextBox")]
    public class TextBoxAutoCompleteTextInput : IAutoCompleteTextInput
    {
        TextBox textBox;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Select(int position, int length)
        {
            textBox.Select(position, length);
        }

        public void SelectAll()
        {
            textBox.SelectAll();
        }

        public int CaretIndex
        {
            get { return textBox.CaretIndex; }
            set { textBox.CaretIndex = value; }
        }

        public int SelectionStart
        {
            get { return textBox.SelectionStart; }
            set { textBox.SelectionStart = value; }
        }

        public int SelectionLength
        {
            get { return textBox.SelectionLength; }
        }

        public string Text
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
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
            var position = textBox.GetRectFromCharacterIndex(charIndex).BottomLeft;
            position.Offset(0, 10); // Vertically pad it. Yeah, Point is mutable. WTF?
            return position;
        }

        public void Focus()
        {
            Keyboard.Focus(textBox);
        }

        public TextBox TextBox
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

        public Thickness Margin
        {
            get { return textBox.Margin; }
            set { textBox.Margin = value; }
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