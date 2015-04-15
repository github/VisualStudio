using System.Windows;
using System.Windows.Controls;
using GitHub.VisualStudio.TeamExplorerConnect;
using GitHub.UI.Helpers;

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
            SharedDictionaryManager.Load("GitHub.UI");
            InitializeComponent();
        }

        void cloneLink_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DoClone();
        }

        void createLink_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DoCreate();
        }

        void signOut_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SignOut();
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
