using System;
using System.Reactive.Linq;
using System.Windows;
using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for PasswordView.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.TwoFactor)]
    public partial class TwoFactorControl : ViewUserControl, IViewFor<ITwoFactorDialogViewModel>, IView, IDisposable
    {
        public TwoFactorControl()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = (ITwoFactorDialogViewModel)e.NewValue;

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.OkCommand, view => view.okButton));
                d(this.OneWayBind(ViewModel, vm => vm.IsBusy, view => view.okButton.ShowSpinner));
                d(this.BindCommand(ViewModel, vm => vm.ResendCodeCommand, view => view.resendCodeButton));

                d(this.Bind(ViewModel, vm => vm.AuthenticationCode, view => view.authenticationCode.Text));

                d(this.OneWayBind(ViewModel, vm => vm.IsAuthenticationCodeSent,
                    view => view.authenticationSentLabel.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.IsSms, view => view.resendCodeButton.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.Description, view => view.description.Text));
                d(this.OneWayBind(ViewModel, vm => vm.InvalidAuthenticationCode, view => view.authenticationFailedLabel.Visibility));
            });
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                {
                    authenticationCode.TryFocus().Subscribe();
                }
            };
        }

        public ITwoFactorDialogViewModel ViewModel
        {
            get { return (ITwoFactorDialogViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel", 
                typeof(ITwoFactorDialogViewModel), 
                typeof(TwoFactorControl), 
                new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ITwoFactorDialogViewModel)value; }
        }

        object IView.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ITwoFactorDialogViewModel)value; }
        }
    }
}
