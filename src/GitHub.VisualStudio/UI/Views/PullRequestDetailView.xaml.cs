using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Services;
using GitHub.UI;
using GitHub.ViewModels;
using GitHub.VisualStudio.UI.Helpers;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericPullRequestDetailView : BusyStateView<IPullRequestDetailViewModel, GenericPullRequestDetailView>
    { }

    [ExportView(ViewType = UIViewType.PRDetail)]
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

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
        }

        void DoOpenOnGitHub()
        {
            var repo = Services.PackageServiceProvider.GetExportedValue<ITeamExplorerServiceHolder>().ActiveRepo;
            var browser = Services.PackageServiceProvider.GetExportedValue<IVisualStudioBrowser>();
            var url = repo.CloneUrl.ToRepositoryUrl().Append("pull/" + ViewModel.Number);
            browser.OpenUrl(url);
        }
    }
}
