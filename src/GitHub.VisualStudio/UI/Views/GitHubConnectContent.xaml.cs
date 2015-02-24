using GitHub.VisualStudio.TeamExplorerConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public PlaceholderGitHubSection ViewModel
        {
            get { return (PlaceholderGitHubSection)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel",
                typeof(PlaceholderGitHubSection),
                typeof(GitHubConnectContent));
    }
}
