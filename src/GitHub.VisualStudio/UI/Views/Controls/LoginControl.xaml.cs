using System.Windows;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using GitHub.Exports;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : IViewFor<ILoginDialog>
    {
        public LoginControl()
        {
            InitializeComponent();
            
            DataContextChanged += (s, e) => ViewModel = (ILoginDialog)e.NewValue;

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.UsernameOrEmail, v => v.usernameOrEmailTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.Password, v => v.passwordTextBox.Text));
                d(this.BindCommand(ViewModel, vm => vm.LoginCmd, v => v.loginButton));
                d(this.BindCommand(ViewModel, vm => vm.CancelCmd, v => v.cancelButton));
            });

            VisualStateManager.GoToState(this, "DotCom", true);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(ILoginDialog), typeof(LoginControl), new PropertyMetadata(null));


        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ILoginDialog)value; }
        }

        public ILoginDialog ViewModel
        {
            [return: AllowNull]
            get { return (ILoginDialog)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
