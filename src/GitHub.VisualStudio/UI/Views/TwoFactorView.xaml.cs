using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GitHub.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using ReactiveUI;

namespace GitHub.Views
{
    /// <summary>
    /// Interaction logic for PasswordView.xaml
    /// </summary>
    [Export(typeof(IViewFor<TwoFactorDialogViewModel>))]
    public partial class TwoFactorView : DialogWindow, IViewFor<TwoFactorDialogViewModel>
    {
        public TwoFactorView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => ViewModel = (TwoFactorDialogViewModel)e.NewValue;
            //IsVisibleChanged += (s, e) => authenticationCode.EnsureFocus();

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.OkCommand, view => view.okButton));
                d(this.BindCommand(ViewModel, vm => vm.CancelCommand, view => view.cancelButton));
                d(this.BindCommand(ViewModel, vm => vm.ShowHelpCommand, view => view.helpButton));
                d(this.BindCommand(ViewModel, vm => vm.ResendCodeCommand, view => view.resendCodeButton));

                d(this.Bind(ViewModel, vm => vm.AuthenticationCode, view => view.authenticationCode.Text));
                d(this.OneWayBind(ViewModel, vm => vm.IsAuthenticationCodeSent,
                    view => view.authenticationSentLabel.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.IsSms, view => view.resendCodeButton.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.Description, view => view.description.Text));
                d(MessageBus.Current.Listen<KeyEventArgs>()
                    .Where(x => ViewModel.IsShowing && x.Key == Key.Escape && !x.Handled)
                    .Subscribe(async key =>
                    {
                        key.Handled = true;
                        await ViewModel.CancelCommand.ExecuteAsync();
                    }));
            });
        }

        public TwoFactorDialogViewModel ViewModel
        {
            get { return (TwoFactorDialogViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel", 
                typeof(TwoFactorDialogViewModel), 
                typeof(TwoFactorView), 
                new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (TwoFactorDialogViewModel)value; }
        }
    }
}
