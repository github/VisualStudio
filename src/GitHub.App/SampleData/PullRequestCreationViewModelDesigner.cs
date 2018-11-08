using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Validation;
using GitHub.ViewModels.GitHubPane;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestCreationViewModelDesigner : PanePageViewModelBase, IPullRequestCreationViewModel
    {
        public PullRequestCreationViewModelDesigner()
        {
            var repositoryModel = new LocalRepositoryModel
            {
                Name = "repo",
                CloneUrl = "http://github.com/user/repo"
            };

            Branches = new List<BranchModel>
            {
                new BranchModel("master", repositoryModel),
                new BranchModel("don/stub-ui", repositoryModel),
                new BranchModel("feature/pr/views", repositoryModel),
                new BranchModel("release-1.0.17.0", repositoryModel),
            }.AsReadOnly();

            TargetBranch = new BranchModel("master", repositoryModel);
            SourceBranch = Branches[2];

            SelectedAssignee = "Haacked (Phil Haack)";
            Users = new List<string>()
            {
                "Haacked (Phil Haack)",
                "shana (Andreia Gaita)"
            };
        }

        public BranchModel SourceBranch { get; set; }
        public BranchModel TargetBranch { get; set; }
        public IReadOnlyList<BranchModel> Branches { get; set; }

        public string SelectedAssignee { get; set; }
        public List<string> Users { get; set; }

        public ReactiveCommand<Unit, IPullRequestModel> CreatePullRequest { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public string PRTitle { get; set; }

        public ReactivePropertyValidator TitleValidator { get; }

        public ReactivePropertyValidator BranchValidator { get; }

        public Task InitializeAsync(LocalRepositoryModel repository, IConnection connection) => Task.CompletedTask;
    }
}