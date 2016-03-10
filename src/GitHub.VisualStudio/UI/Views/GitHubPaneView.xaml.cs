using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using System.Windows.Markup;
using System.Windows.Controls;

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
            DataContextChanged += (s, e) => ViewModel = e.NewValue as GitHubPaneViewModel;
            this.WhenActivated(d =>
            {
            });
        }
    }
}