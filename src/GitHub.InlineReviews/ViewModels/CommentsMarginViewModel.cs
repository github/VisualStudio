using System.Windows.Input;
using ReactiveUI;

namespace GitHub.InlineReviews.ViewModels
{
    public class CommentsMarginViewModel : ReactiveObject
    {
        bool enabled;
        string fileName;
        int commentsInFile;
        bool marginEnabled;

        public CommentsMarginViewModel(ICommand enableInlineComments, ICommand nextInlineComment)
        {
            EnableInlineComments = enableInlineComments;
            NextInlineComment = nextInlineComment;
        }

        public bool Enabled
        {
            get { return enabled; }
            set { this.RaiseAndSetIfChanged(ref enabled, value); }
        }

        public string FileName
        {
            get { return fileName; }
            set { this.RaiseAndSetIfChanged(ref fileName, value); }
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

        public ICommand NextInlineComment { get; }


    }
}
