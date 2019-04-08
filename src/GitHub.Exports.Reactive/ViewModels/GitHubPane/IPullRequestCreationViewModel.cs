using GitHub.Models;
using System.Collections.Generic;
using GitHub.Validation;
using ReactiveUI;
using System.Threading.Tasks;
using System.Reactive;
using GitHub.Services;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestCreationViewModel : IPanePageViewModel
    {
        BranchModel SourceBranch { get; set; }
        BranchModel TargetBranch { get; set; }
        IReadOnlyList<BranchModel> Branches { get; }
        ReactiveCommand<Unit, IPullRequestModel> CreatePullRequest { get; }
        ReactiveCommand<Unit, Unit> Cancel { get; }
        string PRTitle { get; set; }
        ReactivePropertyValidator TitleValidator { get; }
        IAutoCompleteAdvisor AutoCompleteAdvisor { get; }

        Task InitializeAsync(LocalRepositoryModel repository, IConnection connection);
    }
}
