using System.Windows;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using GitHub.UI.Helpers;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Login)]
    public partial class LoginControl : IViewFor<ILoginViewModel>, IView
    {
        public LoginControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");
            Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);

            InitializeComponent();
            
            DataContextChanged += (s, e) => ViewModel = (ILoginViewModel)e.NewValue;

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.UsernameOrEmail, v => v.usernameOrEmailTextBox.Text));
                d(this.Bind(ViewModel, vm => vm.Password, v => v.passwordTextBox.Text));
                d(this.OneWayBind(ViewModel, vm => vm.LoginButtonText, v => v.loginButton.Content));
                d(this.OneWayBind(ViewModel, vm => vm.IsLoginInProgress, v => v.loginButton.IsEnabled, inProgress => !inProgress));
                d(this.BindCommand(ViewModel, vm => vm.LoginCmd, v => v.loginButton));
                d(this.BindCommand(ViewModel, vm => vm.SignUpCommand, v => v.signUpLink));
            });

            VisualStateManager.GoToState(this, "DotCom", true);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(ILoginViewModel), typeof(LoginControl), new PropertyMetadata(null));


        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ILoginViewModel)value; }
        }

        object IView.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ILoginViewModel)value; }
        }

        public ILoginViewModel ViewModel
        {
            [return: AllowNull]
            get { return (ILoginViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
