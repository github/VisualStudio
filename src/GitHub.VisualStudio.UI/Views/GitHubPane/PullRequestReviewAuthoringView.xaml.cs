using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericPullRequestReviewAuthoringView : ViewBase<IPullRequestReviewAuthoringViewModel, GenericPullRequestReviewAuthoringView>
    { }

    [ExportViewFor(typeof(IPullRequestReviewAuthoringViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestReviewAuthoringView : GenericPullRequestReviewAuthoringView
    {
        public PullRequestReviewAuthoringView()
        {
            InitializeComponent();
        }
    }
}
