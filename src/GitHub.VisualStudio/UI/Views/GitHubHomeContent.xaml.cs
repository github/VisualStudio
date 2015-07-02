using System.Windows;
using System.Windows.Controls;
using GitHub.VisualStudio.TeamExplorer.Home;
using GitHub.UI.Helpers;

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

            DataContextChanged += (s, e) => ViewModel = e.NewValue as IGitHubHomeSection;
        }

        public IGitHubHomeSection ViewModel
        {
            get { return (IGitHubHomeSection)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel",
                typeof(IGitHubHomeSection),
                typeof(GitHubHomeContent));
    }
}
