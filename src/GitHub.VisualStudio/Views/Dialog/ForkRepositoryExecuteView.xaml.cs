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

        private void repoForkButton_OnClick(object sender, RoutedEventArgs e)
        {
            ((IForkRepositoryExecuteViewModel)DataContext).CreateFork.Execute(new object());
        }
    }
}
