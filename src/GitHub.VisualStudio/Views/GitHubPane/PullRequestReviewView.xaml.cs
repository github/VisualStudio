using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericPullRequestReviewView : ViewBase<IPullRequestReviewViewModel, GenericPullRequestReviewView>
    { }

    [ExportViewFor(typeof(IPullRequestReviewViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestReviewView : GenericPullRequestReviewView
    {
        public PullRequestReviewView()
        {
            InitializeComponent();
        }
    }
}
