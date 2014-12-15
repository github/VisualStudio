using GitHub.ViewModels;
using Microsoft.VisualStudio.PlatformUI;

namespace GitHub.VisualStudio.UI.Views
{
    /// <summary>
    /// Interaction logic for LoginCommandDialog.xaml
    /// </summary>
    public partial class LoginCommandDialog : DialogWindow
    {
        public LoginCommandDialog(LoginControlViewModel loginControlViewModel)
        {
            InitializeComponent();

            loginControl.ViewModel = loginControlViewModel;
        }
    }
}
