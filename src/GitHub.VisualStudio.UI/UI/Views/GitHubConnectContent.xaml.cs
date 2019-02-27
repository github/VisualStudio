using System.Windows;
using System.Windows.Controls;
using GitHub.VisualStudio.TeamExplorer.Connect;
using System.Windows.Data;
using System.Globalization;
using GitHub.Services;
using GitHub.Models;
using System;
using System.Windows.Input;
using GitHub.VisualStudio.UI.Helpers;

namespace GitHub.VisualStudio.UI.Views
{
    public partial class GitHubConnectContent : UserControl
    {
        public GitHubConnectContent()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as IGitHubConnectSection;
            repositories.PreviewMouseWheel += ScrollViewerUtilities.FixMouseWheelScroll;
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

        private void retry_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Retry();
        }

        void repositories_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ViewModel.OpenRepository();
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

    public class FormatRepositoryNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || parameter as string != "FormatRepositoryName")
                return String.Empty;

            var item = values[0] as LocalRepositoryModel;
            var context = values[1] as IGitHubConnectSection;
            if (item == null)
                return String.Empty;
            if (context == null)
                return item.Name ?? String.Empty;
            
            if (context.SectionConnection.Username == item.CloneUrl.Owner)
                return item.CloneUrl.RepositoryName;
            return item.Name;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IsCurrentRepositoryConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || parameter as string != "IsCurrentRepository")
                return false;

            var item = values[0] as LocalRepositoryModel;
            var context = values[1] as IGitAwareItem;
            return context?.ActiveRepoUri == item?.CloneUrl && String.Equals(context?.ActiveRepo?.LocalPath, item?.LocalPath, StringComparison.OrdinalIgnoreCase);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
