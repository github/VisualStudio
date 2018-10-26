using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.UI.Views
{
    [ExportViewFor(typeof(ISpinnerViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SpinnerView : UserControl
    {
        public SpinnerView()
        {
            InitializeComponent();
        }
    }
}
