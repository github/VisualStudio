using System.Windows;
using System.Windows.Controls;
using GitHub.VisualStudio.TeamExplorerHome;
using GitHub.UI.Helpers;

namespace GitHub.VisualStudio.UI.Views
{
    /// <summary>
    /// Interaction logic for GitHubHomeSection.xaml
    /// </summary>
    public partial class GitHubInvitationContent : UserControl
    {
        public GitHubInvitationContent()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);

            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as IGitHubInvitationSection;
        }

        public IGitHubInvitationSection ViewModel
        {
            get { return (IGitHubInvitationSection)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel",
                typeof(IGitHubInvitationSection),
                typeof(GitHubInvitationContent));
    }
}
