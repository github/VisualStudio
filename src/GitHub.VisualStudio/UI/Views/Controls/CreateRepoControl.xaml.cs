using GitHub.Extensions.Reactive;
using GitHub.UI;
using GitHub.UI.Helpers;
using GitHub.UserErrors;
using NullGuard;
using ReactiveUI;
using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [Export(typeof(IViewFor<ICreateRepoViewModel>))]
    public partial class CreateRepoControl : IViewFor<ICreateRepoViewModel>
    {
        public CreateRepoControl()
        {
            SharedDictionaryManager.Load("GitHub.UI");
            SharedDictionaryManager.Load("GitHub.UI.Reactive");
            Resources.MergedDictionaries.Add(SharedDictionaryManager.SharedDictionary);

            InitializeComponent();

            IObservable<bool> clearErrorWhenChanged = this.WhenAny(
                x => x.ViewModel.RepositoryName,
                x => x.ViewModel.Description,
                (x, y) => new { x, y })
                .WhereNotNull()
                .Select(x => true);

            this.WhenActivated(d =>
            {
                d(this.Bind(ViewModel, vm => vm.RepositoryName, v => v.nameText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.RepositoryNameValidator, v => v.nameValidationMessage.ReactiveValidator));
                d(this.OneWayBind(ViewModel, vm => vm.RepositoryNameWarningText, v => v.safeRepositoryNameWarning.Text));
                d(this.OneWayBind(ViewModel, vm => vm.ShowRepositoryNameWarning, v => v.safeRepositoryNameWarning.ShowError));

                d(this.Bind(ViewModel, vm => vm.Description, v => v.description.Text));
                d(this.Bind(ViewModel, vm => vm.KeepPrivate, v => v.makePrivate.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.CanKeepPrivate, v => v.makePrivate.IsEnabled));

                d(this.OneWayBind(ViewModel, vm => vm.ShowUpgradeToMicroPlanWarning, v => v.upgradeToMicroPlanWarning.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.ShowUpgradePlanWarning, v => v.upgradePlanWarning.Visibility));
                d(this.OneWayBind(ViewModel, vm => vm.SelectedAccount.OwnedPrivateRepos, v => v.ownedPrivateReposText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.SelectedAccount.PrivateReposInPlan, v => v.privateReposInPlanText.Text));

                d(this.OneWayBind(ViewModel, vm => vm.Accounts, v => v.accountsComboBox.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedAccount, v => v.accountsComboBox.SelectedItem));

                d(this.BindCommand(ViewModel, vm => vm.CreateRepository, v => v.createRepositoryButton));
                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.createRepositoryButton.ShowSpinner));
                d(this.BindCommand(ViewModel, vm => vm.UpgradeAccountPlan, v => v.upgradeToMicroLink));
                d(this.BindCommand(ViewModel, vm => vm.UpgradeAccountPlan, v => v.upgradeAccountLink));

                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.nameText.IsEnabled, x => x == false));
                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.description.IsEnabled, x => x == false));
                d(this.OneWayBind(ViewModel, vm => vm.IsPublishing, v => v.accountsComboBox.IsEnabled, x => x == false));

                d(userErrorMessages.RegisterHandler<PublishRepositoryUserError>(clearErrorWhenChanged));
            });
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
           "ViewModel", typeof(ICreateRepoViewModel), typeof(LoginControl), new PropertyMetadata(null));


        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (ICreateRepoViewModel)value; }
        }

        public ICreateRepoViewModel ViewModel
        {
            [return: AllowNull]
            get
            { return (ICreateRepoViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
