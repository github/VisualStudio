using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Documents;

namespace GitHub.VisualStudio.UI.Views.Documents
{
    [ExportViewFor(typeof(IIssueishPaneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class IssueishPaneView : UserControl
    {
        public IssueishPaneView()
        {
            InitializeComponent();
        }
    }
}
