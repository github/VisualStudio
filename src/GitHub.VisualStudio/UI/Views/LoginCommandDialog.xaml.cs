using GitHub.Exports;
using Microsoft.VisualStudio.PlatformUI;

namespace GitHub.VisualStudio.UI.Views
{
    /// <summary>
    /// Interaction logic for LoginCommandDialog.xaml
    /// </summary>
    public partial class LoginCommandDialog : DialogWindow
    {
        public LoginCommandDialog(ILoginDialog loginControlViewModel)
        {
            InitializeComponent();

            loginControl.ViewModel = loginControlViewModel;
        }
    }
}
