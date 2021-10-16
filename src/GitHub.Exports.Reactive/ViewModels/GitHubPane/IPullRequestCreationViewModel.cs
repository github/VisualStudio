using GitHub.Models;
using System.Collections.Generic;
using GitHub.Validation;
using ReactiveUI;
using System.Threading.Tasks;

namespace GitHub.ViewModels.GitHubPane
{
    public interface IPullRequestCreationViewModel : IPanePageViewModel
    {
        IBranch SourceBranch { get; set; }
        IBranch TargetBranch { get; set; }
        IReadOnlyList<IBranch> Branches { get; }
        IReactiveCommand<IPullRequestModel> CreatePullRequest { get; }
        IReactiveCommand<object> Cancel { get; }
        string PRTitle { get; set; }
        ReactivePropertyValidator TitleValidator { get; }

        Task InitializeAsync(ILocalRepositoryModel repository, IConnection connection);
    }
}
