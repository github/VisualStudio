using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.UI;
using GitHub.UI.Helpers;
using GitHub.ViewModels.GitHubPane;
using GitHub.VisualStudio.UI.Helpers;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericPullRequestDetailView : ViewBase<IPullRequestDetailViewModel, GenericPullRequestDetailView>
    { }

    [ExportViewFor(typeof(IPullRequestDetailViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestDetailView : GenericPullRequestDetailView
    {
        public PullRequestDetailView()
        {
            InitializeComponent();

            bodyMarkdown.PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;
            changesSection.PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;

            this.WhenActivated(d =>
            {
                d(ViewModel.OpenOnGitHub.Subscribe(_ => DoOpenOnGitHub()));
            });
        }

        [Import]
        IVisualStudioBrowser VisualStudioBrowser { get; set; }

        void DoOpenOnGitHub()
        {
            var browser = VisualStudioBrowser;
            var cloneUrl = ViewModel.LocalRepository.CloneUrl;
            var url = ToPullRequestUrl(cloneUrl.Host, ViewModel.RemoteRepositoryOwner, ViewModel.LocalRepository.Name, ViewModel.Model.Number);
            browser.OpenUrl(url);
        }

        static Uri ToPullRequestUrl(string host, string owner, string repositoryName, int number)
        {
            var url = string.Format(CultureInfo.InvariantCulture, "https://{0}/{1}/{2}/pull/{3}", host, owner, repositoryName, number);
            return new Uri(url);
        }

        void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            Uri uri;

            if (Uri.TryCreate(e.Parameter?.ToString(), UriKind.Absolute, out uri))
            {
                VisualStudioBrowser.OpenUrl(uri);
            }
        }
    }
}
