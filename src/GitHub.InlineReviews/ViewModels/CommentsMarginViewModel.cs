using System.Windows.Input;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    public class CommentsMarginViewModel : ReactiveObject
    {
        int commentsInFile;
        bool marginEnabled;

        public CommentsMarginViewModel(ICommand enableInlineComments)
        {
            EnableInlineComments = enableInlineComments;
        }

        public int CommentsInFile
        {
            get { return commentsInFile; }
            set { this.RaiseAndSetIfChanged(ref commentsInFile, value); }
        }

        public bool MarginEnabled
        {
            get { return marginEnabled; }
            set { this.RaiseAndSetIfChanged(ref marginEnabled, value); }
        }

        public ICommand EnableInlineComments { get; }
    }
}
