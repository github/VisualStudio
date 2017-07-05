using System;
using System.Windows.Controls;
using System.Windows.Threading;
using GitHub.InlineReviews.ViewModels;

namespace GitHub.InlineReviews.Views
{
    public partial class CommentView : UserControl
    {
        public CommentView()
        {
            InitializeComponent();
            this.Loaded += CommentView_Loaded;
        }

        private void CommentView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (buttonPanel.IsVisible)
            {
                BringIntoView();
                body.Focus();
            }
        }

        private void ReplyPlaceholder_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var command = ((ICommentViewModel)DataContext)?.BeginEdit;

            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
            }
        }

        private void buttonPanel_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (buttonPanel.IsVisible)
            {
                BringIntoView();
            }
        }
    }
}
