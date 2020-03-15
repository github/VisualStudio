using System;
using System.Windows;
using System.Windows.Input;
using GitHub.ViewModels.Dialog;
using Microsoft.VisualStudio.PlatformUI;

namespace GitHub.VisualStudio.Views.Dialog
{
    /// <summary>
    /// The main window for GitHub for Visual Studio's dialog.
    /// </summary>
    public partial class GitHubDialogWindow : DialogWindow
    {
        public GitHubDialogWindow(IGitHubDialogWindowViewModel viewModel)
        {
            DataContext = viewModel;
            viewModel.Done.Subscribe(_ => Close());
            InitializeComponent();
        }

        void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
