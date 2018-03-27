using System.Windows.Input;

namespace GitHub.InlineReviews.ViewModels
{
    public class CommentsMarginViewModel
    {
        public CommentsMarginViewModel(ICommand enableInlineComments)
        {
            EnableInlineComments = enableInlineComments;
        }

        public int CommentsInFile { get; } = 777;

        public ICommand EnableInlineComments { get; }
    }
}
