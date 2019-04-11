using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitHub.UI
{
    public interface IAutoCompleteTextInput : INotifyPropertyChanged
    {
        void Focus();
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Select",
            Justification = "Matches the underlying control method name")]
        void Select(int position, int length);
        void SelectAll();
        int CaretIndex { get; set; }
        int SelectionStart { get; }
        int SelectionLength { get; }
        string Text { get; set; }
        IObservable<EventPattern<KeyEventArgs>> PreviewKeyDown { get; }
        IObservable<EventPattern<RoutedEventArgs>> SelectionChanged { get; }
        IObservable<EventPattern<TextChangedEventArgs>> TextChanged { get; }
        UIElement Control { get; }
        Point GetPositionFromCharIndex(int charIndex);
        Thickness Margin { get; set; }
    }
}