using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels;

namespace GitHub.VisualStudio.Views
{
    [ExportViewFor(typeof(ILabelViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class LabelView : UserControl
    {
        public LabelView()
        {
            InitializeComponent();
        }
    }
}
