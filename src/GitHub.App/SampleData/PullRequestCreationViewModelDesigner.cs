using System.Diagnostics.CodeAnalysis;
using GitHub.ViewModels;
using System.Collections.Generic;
using GitHub.Models;
using GitHub.Validation;
using System;
using System.Windows.Input;
using ReactiveUI;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestCreationViewModelDesigner : BaseViewModel, IPullRequestCreationViewModel
    {
        public PullRequestCreationViewModelDesigner()
        {
            Branches = new List<IBranch>
            {
                new BranchModel { Name = "master" },
                new BranchModel { Name = "don/stub-ui" },
                new BranchModel { Name = "feature/pr/views" },
                new BranchModel { Name = "release-1.0.17.0" }
            }.AsReadOnly();

            TargetBranch = new BranchModel { Name = "master" };
            SourceBranch = Branches[2];

            SelectedAssignee = "Haacked (Phil Haack)";
            Users = new List<string>()
            {
                "Haacked (Phil Haack)",
                "shana (Andreia Gaita)"
            };
        }

        public IBranch SourceBranch { get; set; }
        public IBranch TargetBranch { get; set; }
        public IReadOnlyList<IBranch> Branches { get; set; }

        public string SelectedAssignee { get; set; }
        public List<string> Users { get; set; }

        public IReactiveCommand<IPullRequestModel> CreatePullRequest { get; }

        public string PRTitle { get; set; }

        public ReactivePropertyValidator TitleValidator { get; }

        public ReactivePropertyValidator BranchValidator { get; }
    }
}