using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views.Dialog
{
    [ExportViewFor(typeof(IForkRepositoryExecuteViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ForkRepositoryExecuteView : UserControl
    {
        public ForkRepositoryExecuteView()
        {
            InitializeComponent();
        }
    }
}
