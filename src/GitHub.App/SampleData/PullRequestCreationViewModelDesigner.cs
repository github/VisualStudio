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
            CurrentBranchName = "fix-everything";
            Users = new List<string>()
            {
                "haacked",
                "shana"
            };
        }

        public string CurrentBranchName { get; set; }
        public List<string> Users { get; set; }
    }
}