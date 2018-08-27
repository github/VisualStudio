using System;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericPullRequestCheckView : ViewBase<IPullRequestCheckViewModel, GenericPullRequestCheckView> { }

    [ExportViewFor(typeof(IPullRequestCheckViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestCheckView : GenericPullRequestCheckView
    {
        public PullRequestCheckView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(ViewModel.OpenDetailsUrl.Subscribe(_ => DoOpenDetailsUrl()));
            });
        }

        [Import]
        IVisualStudioBrowser VisualStudioBrowser { get; set; }

        void DoOpenDetailsUrl()
        {
            var browser = VisualStudioBrowser;
            browser.OpenUrl(ViewModel.DetailsUrl);
        }
    }
}
