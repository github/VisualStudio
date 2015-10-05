using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.UI;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Publish)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RepositoryPublishControl : SimpleViewUserControl, IViewFor<IRepositoryPublishViewModel>, IView
    {
        public RepositoryPublishControl()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.Connections, v => v.hostsComboBox.ItemsSource));
                d(this.OneWayBind(ViewModel, vm => vm.IsHostComboBoxVisible, v => v.hostsComboBox.Visibility));
                d(this.Bind(ViewModel, vm => vm.SelectedConnection, v => v.hostsComboBox.SelectedItem));

                d(this.Bind(ViewModel, vm => vm.RepositoryName, v => v.nameText.Text));
                
                d(this.Bind(ViewModel, vm => vm.Description, v => v.description.Text));
                d(this.Bind(ViewModel, vm => vm.KeepPrivate, v => v.makePrivate.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.CanKeepPrivate, v => v.makePrivate.IsEnabled));

                d(this.OneWayBind(ViewModel, vm => vm.Accounts, v => v.accountsComboBox.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedAccount, v => v.accountsComboBox.SelectedItem));

                d(this.BindCommand(ViewModel, vm => vm.PublishRepository, v => v.publishRepositoryButton));

                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.nameText.IsEnabled, x => x == false));
                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.description.IsEnabled, x => x == false));
                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.accountsComboBox.IsEnabled, x => x == false));

                ViewModel.PublishRepository.Subscribe(_ => NotifyDone());

                d(this.WhenAny(x => x.ViewModel.IsPublishing, x => x.Value)
                .Subscribe(x => NotifyIsBusy(x)));

                nameText.Text = ViewModel.DefaultRepositoryName;
            });
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    this.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
           "ViewModel", typeof(IRepositoryPublishViewModel), typeof(RepositoryPublishControl), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IRepositoryPublishViewModel)value; }
        }

        object IView.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IRepositoryPublishViewModel)value; }
        }

        public IRepositoryPublishViewModel ViewModel
        {
            [return: AllowNull]
            get { return (IRepositoryPublishViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
