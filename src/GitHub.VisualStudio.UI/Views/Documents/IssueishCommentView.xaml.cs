using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.ViewModels.Documents;
using GitHub.VisualStudio.UI.Helpers;

namespace GitHub.VisualStudio.Views.Documents
{
    [ExportViewFor(typeof(IIssueishCommentViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class IssueishCommentView : UserControl
    {
        private IDisposable subscription;

        public IssueishCommentView()
        {
            InitializeComponent();
            bodyMarkdown.PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;
            DataContextChanged += HandleDataContextChanged;
        }

        static IVisualStudioBrowser GetBrowser()
        {
            return Services.GitHubServiceProvider.GetService<IVisualStudioBrowser>();
        }

        void DoOpenOnGitHub()
        {
            if (DataContext is IIssueishCommentViewModel vm)
            {
                GetBrowser().OpenUrl(vm.WebUrl);
            }
        }

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            subscription?.Dispose();

            if (e.NewValue is IIssueishCommentViewModel newValue)
            {
                subscription = newValue.OpenOnGitHub.Subscribe(_ => DoOpenOnGitHub());
            }
            else
            {
                subscription = null;
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
