using System;
using System.Reactive;
using System.Windows.Input;
using GitHub.Models;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface ICreateRepoViewModel : IViewModel
    {
        string RepositoryName { get; }
        string SafeRepositoryName { get; }
        bool ShowRepositoryNameWarning { get; }
        string RepositoryNameWarningText { get; }
        ReactivePropertyValidator<string> RepositoryNameValidator { get; }

        string Description { get; set; }

        ReactiveList<IAccount> Accounts { get; }
        IAccount SelectedAccount { get; }

        bool KeepPrivate { get; set; }
        bool CanKeepPrivate { get; }
        bool ShowUpgradeToMicroPlanWarning { get; }
        bool ShowUpgradePlanWarning { get; }

        bool IsPublishing { get; }
        ReactiveCommand<Unit> CreateRepository { get; }
        ReactiveCommand<Object> UpgradeAccountPlan { get; }
        ReactiveCommand<Object> Reset { get; }

        ICommand OkCmd { get; }
        ICommand CancelCmd { get; }
    }
}
