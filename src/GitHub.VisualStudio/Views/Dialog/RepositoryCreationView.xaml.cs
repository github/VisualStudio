using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Windows.Input;
using GitHub.Exports;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.UI;
using GitHub.UserErrors;
using GitHub.ViewModels.Dialog;
using ReactiveUI;

namespace GitHub.VisualStudio.Views.Dialog
{
    public class GenericRepositoryCreationView : ViewBase<IRepositoryCreationViewModel, RepositoryCreationView>
    { }

    /// <summary>
    /// Interaction logic for NewRepositoryCreationView.xaml
    /// </summary>
    [ExportViewFor(typeof(IRepositoryCreationViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class RepositoryCreationView : GenericRepositoryCreationView
    {
        public RepositoryCreationView()
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
                d(this.OneWayBind(ViewModel, vm => vm.RepositoryNameValidator, v => v.nameValidationMessage.ReactiveValidator));
                d(this.OneWayBind(ViewModel, vm => vm.SafeRepositoryNameWarningValidator, v => v.safeRepositoryNameWarning.ReactiveValidator));

                d(this.OneWayBind(ViewModel, vm => vm.BaseRepositoryPathValidator, v => v.pathValidationMessage.ReactiveValidator));

                d(this.BindCommand(ViewModel, vm => vm.CreateRepository, v => v.createRepositoryButton));
                d(this.OneWayBind(ViewModel, vm => vm.IsCreating, v => v.createRepositoryButton.ShowSpinner));

                d(this.BindCommand(ViewModel, vm => vm.BrowseForDirectory, v => v.browsePathButton));

                d(userErrorMessages.RegisterHandler<PublishRepositoryUserError>(clearErrorWhenChanged));
            });
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                    this.TryMoveFocus(FocusNavigationDirection.First).Subscribe();
            };
        }
    }
}
