using GitHub.Models;
using GitHub.UI;
using System.Collections.Generic;

namespace GitHub.ViewModels
{
    public interface IPullRequestCreationViewModel : IReactiveViewModel
    {
        IBranch SourceBranch { get; set; }
        IBranch TargetBranch { get; set; }
        IReadOnlyList<IBranch> Branches { get; }
    }
}
