using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(IIssueListItemViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class IssueListItemView : UserControl
    {
        [ImportingConstructor]
        public IssueListItemView()
        {
            InitializeComponent();
        }
    }
}
