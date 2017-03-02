using GitHub.Models;
using System.Collections.Generic;
using GitHub.Validation;
using ReactiveUI;

namespace GitHub.ViewModels
{
    public interface IPullRequestCreationViewModel : IReactiveDialogViewModel, IPanePageViewModel
    {
        IBranch SourceBranch { get; set; }
        IBranch TargetBranch { get; set; }
        IReadOnlyList<IBranch> Branches { get; }
        IReactiveCommand<IPullRequestModel> CreatePullRequest { get; }
        string PRTitle { get; set; }
        ReactivePropertyValidator TitleValidator { get; }
    }
}
