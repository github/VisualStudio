using System;
using System.Windows.Controls;
using System.Windows.Threading;
using GitHub.InlineReviews.ViewModels;
using GitHub.Services;
using Microsoft.VisualStudio.Shell;
using ReactiveUI;

namespace GitHub.InlineReviews.Views
{
    public class GenericCommentView : GitHub.UI.ViewBase<ICommentViewModel, GenericCommentView> { }

    public partial class CommentView : GenericCommentView
    {
        public CommentView()
        {
            InitializeComponent();
            this.Loaded += CommentView_Loaded;

            this.WhenActivated(d =>
            {
                d(ViewModel.OpenOnGitHub.Subscribe(_ => DoOpenOnGitHub()));
            });
        }

        void DoOpenOnGitHub()
        {
            var serviceProvider = (IGitHubServiceProvider)Package.GetGlobalService(typeof(IGitHubServiceProvider));
            var browser = serviceProvider.GetService<IVisualStudioBrowser>();
            browser.OpenUrl(ViewModel.Thread.GetCommentUrl(ViewModel.Id));
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
