using System.Windows;
using System.Windows.Controls;

namespace GitHub.UI
{
    public partial class GitHubActionLink : Button
    {
        public static readonly DependencyProperty HasDropDownProperty = DependencyProperty.Register(
            "HasDropDown", typeof(bool), typeof(GitHubActionLink));

        public bool HasDropDown
        {
            get { return (bool)GetValue(HasDropDownProperty); }
            set { SetValue(HasDropDownProperty, value); }
        }

        public GitHubActionLink()
        {
        }
    }
}