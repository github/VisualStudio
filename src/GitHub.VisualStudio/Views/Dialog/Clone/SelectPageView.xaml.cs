using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.ViewModels.Dialog.Clone;

namespace GitHub.VisualStudio.Views.Dialog.Clone
{
    [ExportViewFor(typeof(IRepositorySelectViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SelectPageView : UserControl
    {
        public SelectPageView()
        {
            InitializeComponent();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
        }
    }
}
