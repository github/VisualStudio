using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Documents;

namespace GitHub.VisualStudio.Views.Documents
{
    [ExportViewFor(typeof(ICommitListViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class CommitListView : UserControl
    {
        public CommitListView()
        {
            InitializeComponent();
        }
    }
}
