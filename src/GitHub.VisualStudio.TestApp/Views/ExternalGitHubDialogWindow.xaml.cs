using System;
using System.Windows;
using GitHub.ViewModels.Dialog;

namespace GitHub.VisualStudio.Views.Dialog
{
    /// <summary>
    /// The main window for GitHub for Visual Studio's dialog.
    /// </summary>
    public partial class ExternalGitHubDialogWindow : Window
    {
        public ExternalGitHubDialogWindow(IGitHubDialogWindowViewModel viewModel)
        {
            DataContext = viewModel;
            viewModel.Done.Subscribe(_ => Close());
            InitializeComponent();
        }
    }
}
