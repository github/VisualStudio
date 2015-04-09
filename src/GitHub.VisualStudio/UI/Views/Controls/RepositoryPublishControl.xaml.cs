using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.UI;
using GitHub.UI.Helpers;
using GitHub.UserErrors;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Publish)]
    public partial class RepositoryPublishControl : IViewFor<IRepositoryPublishViewModel>, IView, IDisposable
    {
        readonly Subject<object> close;

        public RepositoryPublishControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");
            Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);

            InitializeComponent();

            close = new Subject<object>();

            IObservable<bool> clearErrorWhenChanged = this.WhenAny(
                x => x.ViewModel.RepositoryName,
                x => x.ViewModel.Description,
                (x, y) => new { x, y })
                .WhereNotNull()
                .Select(x => true);

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.RepositoryHosts, v => v.hostsComboBox.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedHost, v => v.hostsComboBox.SelectedItem));

                d(this.Bind(ViewModel, vm => vm.RepositoryName, v => v.nameText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.RepositoryNameValidator, v => v.nameValidationMessage.ReactiveValidator));
                d(this.OneWayBind(ViewModel, vm => vm.SafeRepositoryNameWarningValidator, v => v.safeRepositoryNameWarning.ReactiveValidator));

                d(this.Bind(ViewModel, vm => vm.Description, v => v.description.Text));
                d(this.Bind(ViewModel, vm => vm.KeepPrivate, v => v.makePrivate.IsChecked));
                //d(this.OneWayBind(ViewModel, vm => vm.CanKeepPrivate, v => v.makePrivate.IsEnabled));

                //d(this.OneWayBind(ViewModel, vm => vm.ShowUpgradeToMicroPlanWarning, v => v.upgradeToMicroPlanWarning.Visibility));
                //d(this.OneWayBind(ViewModel, vm => vm.ShowUpgradePlanWarning, v => v.upgradePlanWarning.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.SelectedAccount.OwnedPrivateRepos, v => v.ownedPrivateReposText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.SelectedAccount.PrivateReposInPlan, v => v.privateReposInPlanText.Text));

                d(this.OneWayBind(ViewModel, vm => vm.Accounts, v => v.accountsComboBox.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedAccount, v => v.accountsComboBox.SelectedItem));

                d(this.BindCommand(ViewModel, vm => vm.PublishRepository, v => v.publishRepositoryButton));
                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.publishingSpinner.Visibility));
                //d(this.BindCommand(ViewModel, vm => vm.UpgradeAccountPlan, v => v.upgradeToMicroLink));
                //d(this.BindCommand(ViewModel, vm => vm.UpgradeAccountPlan, v => v.upgradeAccountLink));

                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.nameText.IsEnabled, x => x == false));
                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.description.IsEnabled, x => x == false));
                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.accountsComboBox.IsEnabled, x => x == false));

                d(userErrorMessages.RegisterHandler<PublishRepositoryUserError>(clearErrorWhenChanged));

                ViewModel.PublishRepository.Subscribe(_ => { close.OnNext(null); close.OnCompleted(); });
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

        public IObservable<object> Done { get { return close; } }

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    close.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
