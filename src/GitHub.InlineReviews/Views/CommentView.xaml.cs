using System;
using System.Windows.Controls;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Views
{
    public partial class CommentView : UserControl
    {
        public CommentView()
        {
            InitializeComponent();
        }

        private void ReplyPlaceholder_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            ((ICommentViewModel)DataContext).BeginEdit();
        }
    }
}
