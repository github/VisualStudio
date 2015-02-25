using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using GitHub.UI;
using GitHub.UI.Helpers;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for PasswordView.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.TwoFactor)]
    public partial class TwoFactorControl : IViewFor<ITwoFactorViewModel>
    {
        public TwoFactorControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");
            Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);

            InitializeComponent();
            DataContextChanged += (s, e) => ViewModel = (ITwoFactorViewModel)e.NewValue;
            //IsVisibleChanged += (s, e) => authenticationCode.EnsureFocus();

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.OkCmd, view => view.okButton));
                d(this.BindCommand(ViewModel, vm => vm.CancelCmd, view => view.cancelButton));
                d(this.BindCommand(ViewModel, vm => vm.ShowHelpCmd, view => view.helpButton));
                d(this.BindCommand(ViewModel, vm => vm.ResendCodeCmd, view => view.resendCodeButton));

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
                        await ((ReactiveCommand<object>)ViewModel.CancelCmd).ExecuteAsync();
                    }));
            });
        }

        public ITwoFactorViewModel ViewModel
        {
            get { return (ITwoFactorViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                "ViewModel", 
                typeof(ITwoFactorViewModel), 
                typeof(TwoFactorControl), 
                new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ITwoFactorViewModel)value; }
        }
    }
}
