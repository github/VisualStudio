using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Documents;

namespace GitHub.VisualStudio.UI.Views.Documents
{
    [ExportViewFor(typeof(IIssueDetailViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class IssueDetailView : UserControl
    {
        public IssueDetailView()
        {
            InitializeComponent();
        }
    }
}
