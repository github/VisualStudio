using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.UI;
using GitHub.UserErrors;
using GitHub.ViewModels;
using NullGuard;
using ReactiveUI;
using System.ComponentModel.Composition;

namespace GitHub.VisualStudio.UI.Views.Controls
{
    public class GenericRepositoryCreationControl : SimpleViewUserControl<IRepositoryCreationViewModel, RepositoryCreationControl>
    { }

    /// <summary>
    /// Interaction logic for CloneRepoControl.xaml
    /// </summary>
    [ExportView(ViewType=UIViewType.Create)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RepositoryCreationControl : GenericRepositoryCreationControl
    {
        public RepositoryCreationControl()
        {
            InitializeComponent();

            var clearErrorWhenChanged = this.WhenAny(
                x => x.ViewModel.RepositoryName,
                x => x.ViewModel.Description,
                x => x.ViewModel.BaseRepositoryPath,
                (x, y, z) => new { x, y, z })
                .WhereNotNull()
                .Select(x => true);

            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.GitIgnoreTemplates, v => v.ignoreTemplateList.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedGitIgnoreTemplate, v => v.ignoreTemplateList.SelectedItem));
                d(this.OneWayBind(ViewModel, vm => vm.Licenses, v => v.licenseList.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedLicense, v => v.licenseList.SelectedItem));

                d(this.Bind(ViewModel, vm => vm.RepositoryName, v => v.nameText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.RepositoryNameValidator, v => v.nameValidationMessage.ReactiveValidator));
                d(this.OneWayBind(ViewModel, vm => vm.SafeRepositoryNameWarningValidator, v => v.safeRepositoryNameWarning.ReactiveValidator));

                d(this.Bind(ViewModel, vm => vm.BaseRepositoryPath, v => v.localPathText.Text));
                d(this.OneWayBind(ViewModel, vm => vm.BaseRepositoryPathValidator, v => v.pathValidationMessage.ReactiveValidator));

                d(this.Bind(ViewModel, vm => vm.Description, v => v.description.Text));
                d(this.Bind(ViewModel, vm => vm.KeepPrivate, v => v.makePrivate.IsChecked));
                d(this.OneWayBind(ViewModel, vm => vm.CanKeepPrivate, v => v.makePrivate.IsEnabled));

                d(this.OneWayBind(ViewModel, vm => vm.Accounts, v => v.accountsComboBox.ItemsSource));
                d(this.Bind(ViewModel, vm => vm.SelectedAccount, v => v.accountsComboBox.SelectedItem));

                d(this.BindCommand(ViewModel, vm => vm.CreateRepository, v => v.createRepositoryButton));
                d(this.OneWayBind(ViewModel, vm => vm.IsCreating, v => v.createRepositoryButton.ShowSpinner));

                d(this.BindCommand(ViewModel, vm => vm.BrowseForDirectory, v => v.browsePathButton));

                d(this.OneWayBind(ViewModel, vm => vm.IsCreating, v => v.nameText.IsEnabled, x => x == false));
                d(this.OneWayBind(ViewModel, vm => vm.IsCreating, v => v.description.IsEnabled, x => x == false));
                d(this.OneWayBind(ViewModel, vm => vm.IsCreating, v => v.accountsComboBox.IsEnabled, x => x == false));

                d(userErrorMessages.RegisterHandler<PublishRepositoryUserError>(clearErrorWhenChanged));

                ViewModel.CreateRepository.Subscribe(_ => NotifyDone());
            });
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    this.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };
        }
    }
}
