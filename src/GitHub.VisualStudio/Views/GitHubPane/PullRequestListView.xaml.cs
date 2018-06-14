using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(IPullRequestListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PullRequestListView : UserControl
    {
        [ImportingConstructor]
        public PullRequestListView()
        {
            InitializeComponent();
        }
    }
}
