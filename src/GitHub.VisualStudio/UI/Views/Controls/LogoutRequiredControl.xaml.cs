using GitHub.Exports;
using GitHub.UI;
using GitHub.ViewModels;
using ReactiveUI;
using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericLogoutRequiredControl : SimpleViewUserControl<ILogoutRequiredViewModel, LogoutRequiredControl>
    { }

    /// <summary>
    /// Interaction logic for LogoutRequiredControl.xaml
    /// </summary>
    [ExportView(ViewType = UIViewType.LogoutRequired)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class LogoutRequiredControl : GenericLogoutRequiredControl, IDialogView
    {
        public LogoutRequiredControl()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.BindCommand(ViewModel, vm => vm.Logout, v => v.logoutButton));
                d(this.BindCommand(ViewModel, vm => vm.Cancel, v => v.cancelButton));

                d(ViewModel.Logout
                    .Where(x => x == ProgressState.Success)
                    .Subscribe(_ => NotifyDone()));
                d(ViewModel.CancelCommand.Subscribe(_ => NotifyCancel()));
            });
        }
    }
}
