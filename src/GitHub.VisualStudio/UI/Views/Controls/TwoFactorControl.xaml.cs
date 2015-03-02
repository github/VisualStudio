using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.UI;
using GitHub.UI.Helpers;
using GitHub.ViewModels;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for PasswordView.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.TwoFactor)]
    public partial class TwoFactorControl : IViewFor<ITwoFactorDialogViewModel>, IView
    {
        public TwoFactorControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");
            Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);

            InitializeComponent();
            DataContextChanged += (s, e) => ViewModel = (ITwoFactorDialogViewModel)e.NewValue;

            this.WhenActivated(d =>
            {
                authenticationCode.Focus();

                d(this.BindCommand(ViewModel, vm => vm.OkCommand, view => view.okButton));
                d(this.BindCommand(ViewModel, vm => vm.ResendCodeCommand, view => view.resendCodeButton));

                d(this.Bind(ViewModel, vm => vm.AuthenticationCode, view => view.authenticationCode.Text));
                d(this.OneWayBind(ViewModel, vm => vm.AuthenticationCodeValidator, v => v.authenticationCodeValidationMessage.ReactiveValidator));

                d(this.OneWayBind(ViewModel, vm => vm.IsAuthenticationCodeSent,
                    view => view.authenticationSentLabel.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.IsSms, view => view.resendCodeButton.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.Description, view => view.description.Text));
                d(MessageBus.Current.Listen<KeyEventArgs>()
                    .Where(x => ViewModel.IsShowing && x.Key == Key.Escape && !x.Handled)
                    .Subscribe(key =>
                    {
                        key.Handled = true;
                        //TODO: Hide this dialog.
                    }));
            });
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
