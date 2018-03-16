using System;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericPullRequestListView : ViewBase<IPullRequestListViewModel, PullRequestListView>
    { }

    [ExportViewFor(typeof(IPullRequestListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestListView : GenericPullRequestListView
    {
        [ImportingConstructor]
        public PullRequestListView()
        {
            InitializeComponent();
        }
    }
}
