using System.Windows;
using System.Windows.Controls;
using GitHub.VisualStudio.TeamExplorer.Connect;
using GitHub.UI.Helpers;
using System.Windows.Data;
using System.Globalization;
using GitHub.Services;
using Microsoft.VisualStudio.TeamFoundation.Git.Extensibility;
using GitHub.Models;
using System;

namespace GitHub.VisualStudio.UI.Views
{
    public partial class GitHubConnectContent : UserControl
    {
        public GitHubConnectContent()
        {
            InitializeComponent();
        }

        void cloneLink_Click(object sender, RoutedEventArgs e)
        {
            cloneLink.IsEnabled = false;
            ViewModel.DoClone();
            cloneLink.IsEnabled = true;
        }

        void createLink_Click(object sender, RoutedEventArgs e)
        {
            createLink.IsEnabled = false;
            ViewModel.DoCreate();
            createLink.IsEnabled = true;
        }

        void signOut_Click(object sender, RoutedEventArgs e)
        {
            signOut.IsEnabled = false;
            ViewModel.SignOut();
            signOut.IsEnabled = true;
        }

        void login_Click(object sender, RoutedEventArgs e)
        {
            login.IsEnabled = false;
            ViewModel.Login();
            login.IsEnabled = true;
        }

        void repositories_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
            e.Handled = true;
        }

        public IGitHubConnectSection ViewModel
        {
            get { return (IGitHubConnectSection)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel",
                typeof(IGitHubConnectSection),
                typeof(GitHubConnectContent));
    }

    public class IsCurrentPropertyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values.Length != 2 || !(values[0] is ISimpleRepositoryModel) || !(values[1] is ITeamExplorerServiceHolder) || parameter as string != "IsCurrentRepository")
                    return false;

                var repoInfo = (ISimpleRepositoryModel)values[0];
                var holder = (ITeamExplorerServiceHolder)values[1];
                if (holder.ActiveRepo == null)
                    return false;
                return holder.ActiveRepo.RepositoryPath == repoInfo.LocalPath;
            }
            catch
            {
            }
            return false;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
