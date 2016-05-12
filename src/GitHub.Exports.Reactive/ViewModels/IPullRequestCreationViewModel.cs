using GitHub.Models;
using ReactiveUI;
using System.Collections.Generic;

namespace GitHub.ViewModels
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IPullRequestCreationViewModel : IViewModel
    {
        ObservableCollection<IBranch> Branches { get; }
        ObservableCollection<IAccount> Assignees { get; }
        IAccount SelectedAssignee { get; }
        IBranch TargetBranch { get; }
        IBranch CurrentBranch { get; }

    }
}
