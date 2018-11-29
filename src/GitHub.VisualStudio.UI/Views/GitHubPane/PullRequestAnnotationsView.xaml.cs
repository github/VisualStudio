using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Services;
using GitHub.ViewModels.GitHubPane;
using Microsoft.VisualStudio.Shell;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(IPullRequestAnnotationsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestAnnotationsView : UserControl
    {
        public PullRequestAnnotationsView()
        {
            InitializeComponent();
        }

        IVisualStudioBrowser GetBrowser()
        {
            var serviceProvider = (IGitHubServiceProvider)Package.GetGlobalService(typeof(IGitHubServiceProvider));
            return serviceProvider.GetService<IVisualStudioBrowser>();
        }

        void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            Uri uri;

            if (Uri.TryCreate(e.Parameter?.ToString(), UriKind.Absolute, out uri))
            {
                GetBrowser().OpenUrl(uri);
            }
        }
    }
}
