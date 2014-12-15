using System.Windows;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : IViewFor<LoginControlViewModel>
    {
        public LoginControl()
        {
            InitializeComponent();
            
            DataContextChanged += (s, e) => ViewModel = (LoginControlViewModel)e.NewValue;

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.UsernameOrEmail, v => v.usernameOrEmailTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.Password, v => v.passwordTextBox.Text));
                d(this.BindCommand(ViewModel, vm => vm.LoginCommand, v => v.loginButton));
                d(this.BindCommand(ViewModel, vm => vm.CancelCommand, v => v.cancelButton));
            });

            VisualStateManager.GoToState(this, "DotCom", true);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(LoginControlViewModel), typeof(LoginControl), new PropertyMetadata(null));


        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (LoginControlViewModel)value; }
        }

        public LoginControlViewModel ViewModel
        {
            [return: AllowNull]
            get { return (LoginControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
