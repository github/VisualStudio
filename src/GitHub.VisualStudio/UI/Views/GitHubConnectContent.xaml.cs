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

            DataContextChanged += (s, e) => ViewModel = e.NewValue as IGitHubConnectSection;
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
            if (values.Length != 2 || !(values[0] is ISimpleRepositoryModel) || !(values[1] is IGitAwareItem) || parameter as string != "IsCurrentRepository")
                return false;

            var item = (ISimpleRepositoryModel)values[0];
            var context = (IGitAwareItem)values[1];
            return context.ActiveRepo != null  && context.ActiveRepo.RepositoryPath == item.LocalPath;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FormatRepositoryNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || parameter as string != "FormatRepositoryName")
                return String.Empty;

            if (!(values[1] is IGitAwareItem))
            {
                if (values[0] is ISimpleRepositoryModel)
                    return ((ISimpleRepositoryModel)values[0]).Name;
                return String.Empty;
            }
                
            var item = (ISimpleRepositoryModel)values[0];
            var context = (IGitAwareItem)values[1];
            var uri = context.ActiveRepo?.GetUriFromRepository();
            if (uri != null && uri?.Owner == item.CloneUrl.Owner)
                return item.CloneUrl.RepositoryName;
            return item.Name;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
