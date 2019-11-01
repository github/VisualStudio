using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.VisualStudio.UI.Helpers;
using ReactiveUI;

namespace GitHub.VisualStudio.Views
{
    public class GenericPullRequestReviewCommentView : ViewBase<IPullRequestReviewCommentViewModel, GenericPullRequestReviewCommentView> { }

    [ExportViewFor(typeof(IPullRequestReviewCommentViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestReviewCommentView : GenericPullRequestReviewCommentView
    {
        public PullRequestReviewCommentView()
        {
            InitializeComponent();
            this.Loaded += CommentView_Loaded;
            bodyMarkdown.PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;

            this.WhenActivated(d =>
            {
                d(ViewModel.OpenOnGitHub.Subscribe(_ => DoOpenOnGitHub()));
            });
        }

        static IVisualStudioBrowser GetBrowser()
        {
            return Services.GitHubServiceProvider.GetService<IVisualStudioBrowser>();
        }

        void DoOpenOnGitHub()
        {
            GetBrowser().OpenUrl(ViewModel.WebUrl);
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
            var command = (ICommand)((ICommentViewModel)DataContext)?.BeginEdit;

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

        void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            Uri uri;

            if (Uri.TryCreate(e.Parameter?.ToString(), UriKind.Absolute, out uri))
            {
                GetBrowser().OpenUrl(uri);
            }
        }

        private void body_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var textBox = (PromptTextBox)sender;
            textBox.SelectAll();
        }
    }
}
