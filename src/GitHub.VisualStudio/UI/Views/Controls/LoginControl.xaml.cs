using System;
using System.ComponentModel.Composition;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Controls;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericLoginControl : ViewBase<ILoginControlViewModel, LoginControl>
    { }

    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Login)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class LoginControl : GenericLoginControl
    {
        public LoginControl()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                SetupDotComBindings(d);
                SetupEnterpriseBindings(d);
                SetupSelectedAndVisibleTabBindings(d);
            });

            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    dotComUserNameOrEmail.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };
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
            d(this.OneWayBind(ViewModel, vm => vm.GitHubLogin.NavigatePricing, v => v.pricingLink.Command));
            d(this.OneWayBind(ViewModel, vm => vm.GitHubLogin.Error, v => v.dotComErrorMessage.UserError));
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
            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.NavigateLearnMore, v => v.learnMoreLink.Command));
            d(this.OneWayBind(ViewModel, vm => vm.EnterpriseLogin.Error, v => v.enterpriseErrorMessage.UserError));
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
    }
}
