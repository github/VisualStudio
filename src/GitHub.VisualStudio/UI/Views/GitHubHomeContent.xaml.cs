using System.Windows;
using System.Windows.Controls;

namespace GitHub.VisualStudio.UI.Views
{
    /// <summary>
    /// Interaction logic for GitHubHomeSection.xaml
    /// </summary>
    public partial class GitHubHomeContent : UserControl
    {
        public GitHubHomeContent()
        {
            InitializeComponent();
        }

        public GitHubHomeSection ViewModel
        {
            get { return (GitHubHomeSection)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel",
                typeof(GitHubHomeSection),
                typeof(GitHubHomeContent));
    }
}
