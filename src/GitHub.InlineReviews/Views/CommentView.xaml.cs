using System;
using System.Windows.Input;
using GitHub.InlineReviews.ViewModels;
using GitHub.Services;
using GitHub.UI;
using ReactiveUI;
using System.Threading.Tasks;
using AsyncServiceProvider = Microsoft.VisualStudio.Shell.AsyncServiceProvider;

namespace GitHub.InlineReviews.Views
{
    public class GenericCommentView : ViewBase<ICommentViewModel, GenericCommentView> { }

    public partial class CommentView : GenericCommentView
    {
        public CommentView()
        {
            InitializeComponent();
            this.Loaded += CommentView_Loaded;

            this.WhenActivated(d =>
            {
                d(ViewModel.OpenOnGitHub.Subscribe(async _ => await DoOpenOnGitHub()));
            });
        }

        async Task<IVisualStudioBrowser> GetBrowser()
        {            
            var serviceProvider = (IGitHubServiceProvider) await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(IGitHubServiceProvider));
            return serviceProvider.TryGetMEFComponent<IVisualStudioBrowser>();
        }

        async Task DoOpenOnGitHub()
        {
            var browser = await GetBrowser();
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

        // https://docs.microsoft.com/en-us/previous-versions/windows/apps/hh758286(v=win.10)#event-handlers-that-use-the-async-pattern
        async void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            Uri uri;

            if (Uri.TryCreate(e.Parameter?.ToString(), UriKind.Absolute, out uri))
            {
                var browser = await GetBrowser();
                browser.OpenUrl(uri);
            }
        }
    }
}
