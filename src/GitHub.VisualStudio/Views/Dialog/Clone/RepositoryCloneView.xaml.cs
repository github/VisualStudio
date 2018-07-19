using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Dialog.Clone;

namespace GitHub.VisualStudio.Views.Dialog.Clone
{
    [ExportViewFor(typeof(IRepositoryCloneViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RepositoryCloneView : UserControl
    {
        public RepositoryCloneView()
        {
            InitializeComponent();
        }
    }
}
