using System;
using System.Reactive;
using System.Windows.Input;
using GitHub.Models;
using GitHub.Validation;
using ReactiveUI;
using GitHub.Exports;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Create)]
    public class CreateRepoViewModel : ICreateRepoViewModel
    {
        public string Title { get { return "Create a GitHub Repository"; } } // TODO: this needs to be contextual

        public ReactiveCommand<object> CancelCommand { get; private set; }
        public ICommand CancelCmd { get { return CancelCommand; } }

        public ReactiveCommand<object> OkCommand { get; private set; }
        public ICommand OkCmd { get { return OkCommand; } }

        public string RepositoryName { get; private set; }
        public string SafeRepositoryName { get; private set; }
        public bool ShowRepositoryNameWarning { get; private set; }
        public string RepositoryNameWarningText { get; private set; }
        public ReactivePropertyValidator<string> RepositoryNameValidator { get; private set; }
        public string Description { get; set; }
        public ReactivePropertyValidator<IAccount> SelectedAccountValidator { get; private set; }
        public bool KeepPrivate { get; set; }
        public bool CanKeepPrivate { get; private set; }
        public bool ShowUpgradeToMicroPlanWarning { get; private set; }
        public bool ShowUpgradePlanWarning { get; private set; }
        public ReactiveCommand<Unit> CreateRepository { get; private set; }
        public bool IsPublishing { get; private set; }
        public ReactiveCommand<Object> UpgradeAccountPlan { get; private set; }
        public ReactiveCommand<Object> Reset { get; private set; }
        public ReactiveList<IAccount> Accounts { get; private set; }
        public IAccount SelectedAccount { get; private set; }

    }
}
