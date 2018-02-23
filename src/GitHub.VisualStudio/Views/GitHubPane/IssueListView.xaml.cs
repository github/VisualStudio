using System;
using System.ComponentModel.Composition;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    public class GenericIssueListView : ViewBase<IIssueListViewModel, IssueListView>
    { }

    [ExportViewFor(typeof(IIssueListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class IssueListView : GenericIssueListView
    {
        [ImportingConstructor]
        public IssueListView()
        {
            InitializeComponent();
        }
    }
}
