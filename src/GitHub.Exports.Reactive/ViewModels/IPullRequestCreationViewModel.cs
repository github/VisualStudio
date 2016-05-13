using GitHub.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace GitHub.ViewModels
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IPullRequestCreationViewModel : IViewModel
    {
        IReadOnlyList<IBranchModel> Branches { get; }
        IReadOnlyList<IAccount> Assignees { get; }
        IAccount SelectedAssignee { get; }
        IBranchModel TargetBranch { get; }
        IBranchModel CurrentBranch { get; }

    }
}
