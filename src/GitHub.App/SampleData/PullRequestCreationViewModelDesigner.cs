using System.Diagnostics.CodeAnalysis;
using GitHub.ViewModels;
using System.Collections.Generic;

namespace GitHub.SampleData
{
    [ExcludeFromCodeCoverage]
    public class PullRequestCreationViewModelDesigner : BaseViewModelDesigner, IPullRequestCreationViewModel
    {
        public PullRequestCreationViewModelDesigner()
        {
            Branches = new List<string>()
            {
                "don/stub-ui",
                "feature/pr/views",
                "release-1.0.17.0"
            };

            CurrentBranchName = "fix-everything";
            SelectedAssignee = "Haacked (Phil Haack)";
            TargetBranchName = "master";
            Users = new List<string>()
            {
                "Haacked (Phil Haack)",
                "shana (Andreia Gaita)"
            };
        }

        public string CurrentBranchName { get; set; }
        public string SelectedAssignee { get; set; }
        public string TargetBranchName { get; set; }
        public List<string> Branches { get; set; }
        public List<string> Users { get; set; }
    }
}