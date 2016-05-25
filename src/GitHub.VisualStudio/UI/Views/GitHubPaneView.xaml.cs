using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views
{
    public class GenericGitHubPaneView : SimpleViewUserControl<IGitHubPaneViewModel, GitHubPaneView>
    {
    }

    [ExportView(ViewType = UIViewType.GitHubPane)]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public partial class GitHubPaneView : GenericGitHubPaneView
    {
        public GitHubPaneView()
        {
            this.InitializeComponent();
            this.WhenActivated(d =>
            {
            });
        }
    }
}