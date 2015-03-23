using System;
using System.Reactive.Linq;
using System.Windows;
using GitHub.Controls;
using GitHub.Exports;
using GitHub.UI;
using GitHub.UI.Helpers;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using System.Reactive.Subjects;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Login)]
    public partial class LoginControl : IViewFor<ILoginControlViewModel>, IView
    {
        public LoginControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");
            Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);

            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = (ILoginControlViewModel)e.NewValue;
            close = new Subject<object>();

            this.WhenActivated(d =>
            {
                SetupDotComBindings(d);
                SetupEnterpriseBindings(d);
                SetupSelectedAndVisibleTabBindings(d);
                d(ViewModel.AuthenticationResults
                    .Subscribe(ret =>
                {
                    if (ret == Authentication.AuthenticationResult.Success)
                    {
                        close.OnNext(null);
                        close.OnCompleted();
                    }
                }));
            });
        }
        
        void SetupDotComBindings(Action<IDisposable> d)
        {
            d(this.OneWayBind(ViewModel, vm => vm.GitHubLogin.IsLoggingIn, x => x.dotComloginControlsPanel.IsEnabled, x => x == false));

            d(this.Bind(ViewModel, vm => vm.GitHubLogin.UsernameOrEmail, x => x.dotComUserNameOrEmail.Text));
            d(this.OneWayBind(ViewModel, vm => vm.GitHubLogin.UsernameOrEmailValidator, v => v.dotComUserNameOrEmailValidationMessage.ReactiveValidator));

            d(this.BindPassword(ViewModel, vm => vm.GitHubLogin.Password, v => v.dotComPassword.Text, dotComPassword));
            d(this.OneWayBind(ViewModel, vm => vm.GitHubLogin.PasswordValidator, v => v.dotComPasswordValidationMessage.ReactiveValidator));

            d(this.OneWayBind(ViewModel, vm => vm.GitHubLogin.Login, v => v.dotComLogInButton.Command));
            d(this.OneWayBind(ViewModel, vm => vm.GitHubLogin.IsLoggingIn, v => v.dotComLogInButton.ShowSpinner));

            d(this.OneWayBind(ViewModel, vm => vm.GitHubLogin.ForgotPassword, v => v.dotComForgotPasswordLink.Command));

            d(this.OneWayBind(ViewModel, vm => vm.GitHubLogin.ShowLogInFailedError, v => v.dotComLoginFailedMessage.Visibility));
            d(this.OneWayBind(ViewModel, vm => vm.GitHubLogin.ShowTwoFactorAuthFailedError, v => v.dotComTwoFactorAuthFailedMessage.Visibility));
        }

        void SetupEnterpriseBindings(Action<IDisposable> d)
        {
            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.IsLoggingIn, x => x.enterpriseloginControlsPanel.IsEnabled, x => x == false));

            d(this.Bind(ViewModel, vm => vm.EnterpriseLogin.UsernameOrEmail, x => x.enterpriseUserNameOrEmail.Text));
            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.UsernameOrEmailValidator, v => v.enterpriseUserNameOrEmailValidationMessage.ReactiveValidator));

            d(this.BindPassword(ViewModel, vm => vm.EnterpriseLogin.Password, v => v.enterprisePassword.Text, enterprisePassword));
            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.PasswordValidator, v => v.enterprisePasswordValidationMessage.ReactiveValidator));

            d(this.Bind(ViewModel, vm => vm.EnterpriseLogin.EnterpriseUrl, v => v.enterpriseUrl.Text));
            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.EnterpriseUrlValidator, v => v.enterpriseUrlValidationMessage.ReactiveValidator));

            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.Login, v => v.enterpriseLogInButton.Command));
            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.IsLoggingIn, v => v.enterpriseLogInButton.ShowSpinner));

            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.ForgotPassword, v => v.enterpriseForgotPasswordLink.Command));

            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.ShowLogInFailedError, v => v.enterpriseLoginFailedMessage.Visibility));
            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.ShowTwoFactorAuthFailedError, v => v.enterpriseTwoFactorAuthFailedMessage.Visibility));
            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.ShowConnectingToHostFailed, v => v.enterpriseConnectingFailedMessage.Visibility));
        }

        void SetupSelectedAndVisibleTabBindings(Action<IDisposable> d)
        {
            d(this.WhenAny(x => x.ViewModel.LoginMode, x => x.Value)
                .Select(x => x == LoginMode.DotComOrEnterprise || x == LoginMode.DotComOnly)
                .BindTo(this, v => v.dotComTab.IsEnabled));

            d(this.WhenAny(x => x.ViewModel.LoginMode, x => x.Value)
                .Select(x => x == LoginMode.DotComOrEnterprise || x == LoginMode.EnterpriseOnly)
                .BindTo(this, v => v.enterpriseTab.IsEnabled));

            d(this.WhenAny(x => x.ViewModel.LoginMode, x => x.Value)
                .Select(x => x == LoginMode.DotComOrEnterprise || x == LoginMode.DotComOnly)
                .Where(x => x == true)
                .BindTo(this, v => v.dotComTab.IsSelected));

            d(this.WhenAny(x => x.ViewModel.LoginMode, x => x.Value)
                .Select(x => x == LoginMode.EnterpriseOnly)
                .Where(x => x == true)
                .BindTo(this, v => v.enterpriseTab.IsSelected));
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(ILoginControlViewModel), typeof(LoginControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ILoginControlViewModel)value; }
        }

        object IView.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ILoginControlViewModel)value; }
        }

        public ILoginControlViewModel ViewModel
        {
            [return: AllowNull]
            get { return (ILoginControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        Subject<object> close;
        public IObservable<object> Done { get { return close; } }
    }
}
