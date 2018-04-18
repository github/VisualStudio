using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views.Dialog
{
    [ExportViewFor(typeof(IForkRepositorySwitchViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ForkRepositorySwitchView : UserControl
    {
        public ForkRepositorySwitchView()
        {
            InitializeComponent();
        }

        private void repoSwitchButton_OnClick(object sender, RoutedEventArgs e)
        {
            ((IForkRepositorySwitchViewModel)DataContext).SwitchFork.Execute(new object());
        }
    }
}
