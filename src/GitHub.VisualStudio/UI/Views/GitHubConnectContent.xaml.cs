using System.Windows;
using System.Windows.Controls;
using GitHub.VisualStudio.TeamExplorerConnect;

namespace GitHub.VisualStudio.UI.Views
{
    /// <summary>
    /// This is a temporary placeholder class until the VS Connect section
    /// is done
    /// </summary>
    public partial class GitHubConnectContent : UserControl
    {
        public GitHubConnectContent()
        {
            InitializeComponent();
        }

        private void cloneLink_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DoClone();
        }

        private void createLink_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DoCreate();
        }

        private void m_Click(object sender, RoutedEventArgs e)
        {

        }

        public GitHubConnectSection ViewModel
        {
            get { return (GitHubConnectSection)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel",
                typeof(GitHubConnectSection),
                typeof(GitHubConnectContent));
    }
}
