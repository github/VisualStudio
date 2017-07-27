using System.ComponentModel;

namespace GitHub.InlineReviews.ViewModels
{
    public interface IGlyphMarginViewModel : INotifyPropertyChanged
    {
        bool Visible { get; }
    }
}
