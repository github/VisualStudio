using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(IPullRequestUserReviewsViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestUserReviewsView : UserControl
    {
        public PullRequestUserReviewsView()
        {
            InitializeComponent();
        }
    }
}
