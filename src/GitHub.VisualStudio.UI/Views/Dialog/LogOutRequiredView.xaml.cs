using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views.Dialog
{
    [ExportViewFor(typeof(ILogOutRequiredViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class LogOutRequiredView : UserControl
    {
        public LogOutRequiredView()
        {
            InitializeComponent();
        }
    }
}
