using GitHub.Models;
using GitHub.UI;
using System.Collections.Generic;
using System.Windows.Input;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IPullRequestCreationViewModel : IReactiveViewModel
    {
        IBranch SourceBranch { get; set; }
        IBranch TargetBranch { get; set; }
        IReadOnlyList<IBranch> Branches { get; }
        IReactiveCommand<IPullRequestModel> CreatePullRequest { get; }
        string PRTitle { get; set; }
        ReactivePropertyValidator TitleValidator { get; }
    }
}
