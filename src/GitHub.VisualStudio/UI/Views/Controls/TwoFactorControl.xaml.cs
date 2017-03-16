using System;
using System.Reactive.Linq;
using System.Windows;
using GitHub.Exports;
using GitHub.Extensions.Reactive;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericTwoFactorControl : SimpleViewUserControl<ITwoFactorDialogViewModel, TwoFactorControl>
    { }

    /// <summary>
    /// Interaction logic for PasswordView.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.TwoFactor)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class TwoFactorControl : GenericTwoFactorControl
    {
        public TwoFactorControl()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.OkCommand, view => view.okButton));
                d(this.OneWayBind(ViewModel, vm => vm.IsBusy, view => view.okButton.ShowSpinner));
                d(this.BindCommand(ViewModel, vm => vm.ResendCodeCommand, view => view.resendCodeButton));

                d(this.Bind(ViewModel, vm => vm.AuthenticationCode, view => view.authenticationCode.Text));
                d(this.OneWayBind(ViewModel, vm => vm.NavigateLearnMore, view => view.twoFactorReadMoreLink.Command));
                d(this.OneWayBind(ViewModel, vm => vm.IsAuthenticationCodeSent,
                    view => view.authenticationSentLabel.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.IsSms, view => view.resendCodeButton.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.Description, view => view.description.Text));
                d(this.OneWayBind(ViewModel, vm => vm.ShowErrorMessage, view => view.authenticationFailedLabel.Visibility));
                d(this.ViewModel.ResendCodeCommand.Subscribe(_ => SetFocus()));
                d(this.ViewModel.OkCommand.Subscribe(_ => SetFocus()));
            });
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                {
                    SetFocus();
                }
            };
        }

        void SetFocus()
        {
            authenticationCode.TryFocus().Subscribe();
        }
    }
}
