using System.ComponentModel.Composition;
using System.Windows.Controls;
using GitHub.Exports;
using GitHub.ViewModels.GitHubPane;

namespace GitHub.VisualStudio.Views.GitHubPane
{
    [ExportViewFor(typeof(ILoginFailedViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class LoginFailedView : UserControl
    {
        public LoginFailedView()
        {
            InitializeComponent();
        }
    }
}
